using DotnetSpider;
using DotnetSpider.Core;
using DotnetSpider.Data;
using DotnetSpider.Data.Parser;
using DotnetSpider.Data.Storage;
using DotnetSpider.Downloader;
using DotnetSpider.MessageQueue;
using DotnetSpider.Statistics;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace KekeSpider
{
    public class GithubSpider : Spider
    {
        class Parser : DataParserBase
        {
            protected override Task<DataFlowResult> Parse(DataFlowContext context)
            {
                var selectable = context.GetSelectable();
                // 解析数据
                var name = selectable.XPath("//*[@id=\"subject_list\"]/ul/li[1]/div[2]/h2/a")
                    .GetValue();
                var author = selectable.XPath("//*[@id=\"subject_list\"]/ul/li[1]/div[2]/div[1]")
                    .GetValue();
                context.AddItem("author", author);
                context.AddItem("username", name);

                // 添加目标链接
                var urls = selectable.Links().Regex("(https://book.douban\\.com/tag/[\\w\\-]+)").GetValues();
                AddTargetRequests(context, urls);

                // 如果解析为空，跳过后续步骤(存储 etc)
                if (string.IsNullOrWhiteSpace(name))
                {
                    context.ClearItems();
                    return Task.FromResult(DataFlowResult.Terminated);
                }

                return Task.FromResult(DataFlowResult.Success);
            }
        }

        public GithubSpider(IMessageQueue mq, IStatisticsService statisticsService, ISpiderOptions options,
            ILogger<Spider> logger, IServiceProvider services) : base(mq, statisticsService, options, logger, services)
        {
        }

        protected override void Initialize()
        {
            NewGuidId();
            RetryDownloadTimes = 3;
            DownloaderSettings.Timeout = 5000;
            DownloaderSettings.Type = DownloaderType.HttpClient;
            AddDataFlow(new Parser()).AddDataFlow(new ConsoleStorage());
            AddRequests("https://book.douban.com/tag/%E7%BC%96%E7%A8%8B");
        }
    }
}
