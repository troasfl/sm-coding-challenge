using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using sm_coding_challenge.Models;
using sm_coding_challenge.Services.DataProvider;

namespace sm_coding_challenge
{
    public class DataUpdateWorker : BackgroundService
    {
        private readonly ILogger<DataUpdateWorker> _logger;
        private readonly ICustomHttpClient _client;
        private readonly IPlayersCacheRepository _playersCacheRepository;

        public DataUpdateWorker(IConfiguration configuration, ILogger<DataUpdateWorker> logger, ICustomHttpClient client, IPlayersCacheRepository playersCacheRepository)
        {
            Configuration = configuration;
            _logger = logger;
            _client = client;
            _playersCacheRepository = playersCacheRepository;
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation($"{nameof(DataUpdateWorker)} started at: {DateTime.UtcNow}");
            return base.StartAsync(cancellationToken);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation($"{nameof(DataUpdateWorker)} executing task.");
            while (!stoppingToken.IsCancellationRequested)
            {
                PerformTask();
                await Task.Delay(TimeSpan.FromDays(7), stoppingToken);
            }
        }

        private void PerformTask()
        {
            try
            {
                _logger.LogInformation("About to perform task.");
                 RefreshCache();
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Exception: {e}");
            }
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogWarning($"{nameof(DataUpdateWorker)} stopped at: {DateTime.UtcNow}");

            return base.StopAsync(cancellationToken);
        }

        public override void Dispose()
        {
            _logger.LogWarning($"{nameof(DataUpdateWorker)} disposed at: {DateTime.UtcNow}");

            base.Dispose();
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

                    _logger.LogInformation("Data loaded from api and cached.");
                }
                else
                {
                    _logger.LogWarning("No data available for cache.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.ToString());
            }
        }

        public IConfiguration Configuration { get; }

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

            _logger.LogInformation($"player {playerModel.Id} Rushing stats added.");
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

            _logger.LogInformation($"player {playerModel.Id} Kicking stats added.");
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

            _logger.LogInformation($"player {playerModel.Id} Passing stats added.");
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

            _logger.LogInformation($"player {playerModel.Id} Receiving stats added.");
        }

    }
}