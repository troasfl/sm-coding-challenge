using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using AspNetCoreRateLimit;
using CacheManager.Core;
using CacheManager.Core.Internal;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using sm_coding_challenge.Models;
using sm_coding_challenge.Services.DataProvider;

namespace sm_coding_challenge
{
    public class Startup
    {
        private ICustomHttpClient _client;
        private IPlayersCacheRepository _playersCacheRepository;
        private ILogger<Startup> _logger;

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews(options =>
            {
                options.AllowEmptyInputInBodyModelBinding = true;
            }).AddNewtonsoftJson(options =>
            {
                options.SerializerSettings.ContractResolver = new DefaultContractResolver
                {
                    NamingStrategy = new SnakeCaseNamingStrategy()
                };
            }); 
            
            // needed to load configuration from appsettings.json
            services.AddOptions();

            // register the data provider
            services.AddTransient<IDataProvider, DataProviderImpl>();

            // register my custom http client
            services.AddHttpClient<ICustomHttpClient, CustomHttpClient>()
                .ConfigurePrimaryHttpMessageHandler(handler =>
                    new HttpClientHandler
                    {
                        AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
                    });
            
            // in memory caching service added. NB: distributed cache such as redis will be used in production
            services.AddMemoryCache();

            //load general configuration from appsettings.json
            services.Configure<IpRateLimitOptions>(Configuration.GetSection("IpRateLimiting"));

            // inject counter and rules stores
            services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();
            services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();

            // the clientId/clientIp resolvers use it.
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            // configuration (resolvers, counter key builders)
            services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();

            var config = new CacheManager.Core.ConfigurationBuilder()
                .WithMicrosoftMemoryCacheHandle("PlayersCache", new MemoryCacheOptions())
                .WithExpiration(ExpirationMode.Absolute, TimeSpan.FromDays(7))
                .Build();

            services.AddSingleton<IPlayersCacheRepository>(serviceProvider => new PlayersCacheRepository(new BaseCacheManager<PlayerModel>(config)));

            CommonCachingService.RawDataCache = new BaseCacheManager<DataResponseModel>(config);
            CommonCachingService.RawDataCache.OnRemoveByHandle += RawDataCacheOnOnRemoveByHandle;
        }

        private void RawDataCacheOnOnRemoveByHandle(object? sender, CacheItemRemovedEventArgs e)
        {
             RefreshCache();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IServiceProvider serviceProvider)
        {
            _playersCacheRepository = serviceProvider.GetRequiredService<IPlayersCacheRepository>();
            _client = serviceProvider.GetRequiredService<ICustomHttpClient>();
            _logger = serviceProvider.GetRequiredService<ILogger<Startup>>();
            
            RefreshCache();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseIpRateLimiting();

            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }

        public async void RefreshCache()
        {
            try
            {
                var key = Configuration["CacheKey"];
                var cts = new CancellationTokenSource();

                var url = Configuration["Endpoint"];

                var response = await _client.MakeGetRequestAsync(url, cts.Token);
                var stringData = await response.Content.ReadAsStringAsync();
                var data = JsonConvert.DeserializeObject<DataResponseModel>(stringData, new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.Auto
                });

                if (data != null)
                {
                    CommonCachingService.RawDataCache.Put(key, data);

                    var successListOfTasks = new List<Task>();

                    // cache data for each player for rushing stats
                    foreach (var player in data.Rushing)
                        successListOfTasks.Add(DoCacheRushingAsync(player));
                    await Task.WhenAll(successListOfTasks);
                    successListOfTasks.Clear();

                    // cache data for each player for kicking stats
                    foreach (var player in data.Kicking)
                        successListOfTasks.Add(DoCacheKickingAsync(player));
                    await Task.WhenAll(successListOfTasks);
                    successListOfTasks.Clear();

                    // cache data for each player for passing stats
                    foreach (var player in data.Passing)
                        successListOfTasks.Add(DoCachePassingAsync(player));
                    await Task.WhenAll(successListOfTasks);
                    successListOfTasks.Clear();

                    // cache data for each player for receiving stats
                    foreach (var player in data.Receiving)
                        successListOfTasks.Add(DoCacheReceivingAsync(player));
                    await Task.WhenAll(successListOfTasks);
                    successListOfTasks.Clear();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.ToString());
            }
        }
        private async Task DoCacheRushingAsync(PlayerBaseRushingStatsModel player)
        {
            var playerModel = await _playersCacheRepository.Get(player.Id) ?? new PlayerModel(player.Id, player.EntryId, player.Name, player.Position);

            playerModel.Rushing = new RushingModel
            {
                Att = player.Att,
                Fum = player.Fum,
                Tds = player.Tds,
                Yds = player.Yds
            };

            _playersCacheRepository.Put(player.Id, playerModel);
        }
        private async Task DoCacheKickingAsync(PlayerBaseKickingStatsModel player)
        {
            var playerModel = await _playersCacheRepository.Get(player.Id) ?? new PlayerModel(player.Id, player.EntryId, player.Name, player.Position);

            playerModel.Kicking = new KickingModel
            {
                ExtraPtAtt = player.ExtraPtAtt,
                ExtraPtMade = player.ExtraPtMade,
                FldGoalsAtt = player.FldGoalsAtt,
                FldGoalsMade = player.FldGoalsMade
            };

            _playersCacheRepository.Put(player.Id, playerModel);
        }
        private async Task DoCachePassingAsync(PlayerBasePassingStatsModel player)
        {
            var playerModel = await _playersCacheRepository.Get(player.Id) ?? new PlayerModel(player.Id, player.EntryId, player.Name, player.Position);

            playerModel.Passing = new PassingModel
            {
                Tds = player.Tds,
                Att = player.Att,
                Cmp = player.Cmp,
                Int = player.Int,
                Yds = player.Yds
            };

            _playersCacheRepository.Put(player.Id, playerModel);
        }
        private async Task DoCacheReceivingAsync(PlayerBaseReceivingStatsModel player)
        {
            var playerModel = await _playersCacheRepository.Get(player.Id) ?? new PlayerModel(player.Id, player.EntryId, player.Name, player.Position);

            playerModel.Receiving = new ReceivingModel
            {
                Tds = player.Tds,
                Yds = player.Yds,
                Rec = player.Rec
            };

            _playersCacheRepository.Put(player.Id, playerModel);
        }
    }
}
