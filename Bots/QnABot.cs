// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.AI.QnA;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Microsoft.BotBuilderSamples
{
    public class QnABot : ActivityHandler
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<QnABot> _logger;
        private readonly IHttpClientFactory _httpClientFactory;

        public QnABot(IConfiguration configuration, ILogger<QnABot> logger, IHttpClientFactory httpClientFactory)
        {
            _configuration = configuration;
            _logger = logger;
            _httpClientFactory = httpClientFactory;
        }

        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            var httpClient = _httpClientFactory.CreateClient();

            var qnaMaker = new QnAMaker(new QnAMakerEndpoint
            {
                KnowledgeBaseId = _configuration["QnAKnowledgebaseId"],
                EndpointKey = _configuration["QnAEndpointKey"],
                Host = _configuration["QnAEndpointHostName"]
            },
            null,
            httpClient);

            _logger.LogInformation("Calling QnA Maker");

            var options = new QnAMakerOptions { Top = 1 };

            // The actual call to the QnA Maker service.
            var response = await qnaMaker.GetAnswersAsync(turnContext, options);
            if (response != null && response.Length > 0)
            {
                await turnContext.SendActivityAsync(MessageFactory.Text(response[0].Answer), cancellationToken);
            }
            else
            {
                await CreateTicket(turnContext, cancellationToken);
            }
        }



        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            foreach (var member in membersAdded)
            {
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    await turnContext.SendActivityAsync(MessageFactory.Text($"Hello and welcome! I am here to help you with FAQ. Please ask me a question."), cancellationToken);
                }
            }
        }

        private static async Task CreateTicket(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            var message = turnContext.Activity.Text;
            //make a Rest call
            await Task.Delay(5000);
            await turnContext.SendActivityAsync(MessageFactory.Text("I will need to esclate this issue to a support personal. I am creating a support ticket for you. I will provide you with the ticket number momentarily."), cancellationToken);
            await Task.Delay(10000);
            await turnContext.SendActivityAsync(MessageFactory.Text("Your ticket number is 123456. A level 2 support personal will contact you within an hour."), cancellationToken);
            await Task.Delay(3000);
            await turnContext.SendActivityAsync(MessageFactory.Text("Thank you for giving me an oppotunity to assist you today!"), cancellationToken);
            await Task.Delay(3000);
            await turnContext.SendActivityAsync(MessageFactory.Text("Is there anything else that I could help you with today?"), cancellationToken);



        }

    }
}
