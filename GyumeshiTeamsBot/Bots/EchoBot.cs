// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
//
// Generated with Bot Builder V4 SDK Template for Visual Studio EchoBot v4.14.0

using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Azure.CognitiveServices.Vision.CustomVision.Prediction;
using Microsoft.Azure.CognitiveServices.Vision.CustomVision.Prediction.Models;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace GyumeshiTeamsBot.Bots
{
    public class EchoBot : ActivityHandler
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ICustomVisionPredictionClient _customVisionPredictionClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<EchoBot> _logger;

        public EchoBot(IHttpClientFactory httpClientFactory, ICustomVisionPredictionClient customVisionPredictionClient, IConfiguration configuration, ILogger<EchoBot> logger)
        {
            _httpClientFactory = httpClientFactory;
            _customVisionPredictionClient = customVisionPredictionClient;
            _configuration = configuration;
            _logger = logger;
        }

        public override Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation($"OnTurnAsync: {turnContext.Activity.Type}");
            return base.OnTurnAsync(turnContext, cancellationToken);
        }

        // ユーザがなにかメッセージを送ってくれた時に必ず呼ばれるメソッド
        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            string replyText = default;

            if (Uri.TryCreate(turnContext.Activity.Text, UriKind.Absolute, out var imageUrl))
            {
                var customVision = _configuration.GetSection("CustomVisionConfiguration");
                var projectId = customVision.GetValue<string>("ProjectId");
                var publishedName = customVision.GetValue<string>("PublishedName");
                var classifiedResult = await _customVisionPredictionClient.ClassifyImageUrlAsync(
                    new Guid(projectId),
                    publishedName,
                    new ImageUrl(imageUrl.ToString())
                    );
                var prediction = classifiedResult.Predictions.FirstOrDefault();
                if (prediction != null && prediction.Probability > 0.75)
                {
                    replyText = $"これは {prediction.TagName} だと思います。（自信：{prediction.Probability:##0.##%}）";
                }
                else
                {
                    replyText = "わかりませんでした💦";
                }
            }
            else
            {
                replyText = "画像 URL を送ってください";
            }

            await turnContext.SendActivityAsync(MessageFactory.Text(replyText, replyText), cancellationToken);
        }

        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            var welcomeText = "こんにちは。画像の URL を送ってください";
            foreach (var member in membersAdded)
            {
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    await turnContext.SendActivityAsync(MessageFactory.Text(welcomeText, welcomeText), cancellationToken);
                }
            }
        }
    }
}
