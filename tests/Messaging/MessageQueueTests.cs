using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xunit;

using Zongsoft.Externals.Aliyun;
using Zongsoft.Externals.Aliyun.Messaging;

namespace Zongsoft.Externals.Aliyun.Tests.Messaging
{
	public class MessageQueueTests
	{
		#region 常量定义
		private const string EXISTS_QUEUE_NAME = "default";
		#endregion

		#region 成员字段
		private MessageQueueProvider _provider;
		#endregion

		#region 构造函数
		public MessageQueueTests()
		{
			var configuration = Zongsoft.Options.Configuration.OptionConfiguration.Load(@"\Zongsoft\Zongsoft.Externals.Aliyun\src\Zongsoft.Externals.Aliyun.option");
			var option = configuration.GetOptionObject("Externals/Aliyun/General") as Zongsoft.Externals.Aliyun.Options.Configuration.GeneralConfiguration;

			_provider = new MessageQueueProvider(option);
		}
		#endregion

		#region 测试方法
		[Xunit.Fact]
		public void CountTest()
		{
			var queue = _provider.GetQueue(EXISTS_QUEUE_NAME);
			Assert.NotNull(queue);

			var count = queue.Count;
		}

		[Xunit.Fact]
		public void EnqueueTest()
		{
			var queue = _provider.GetQueue(EXISTS_QUEUE_NAME);
			Assert.NotNull(queue);

			queue.Enqueue("How are you?!");
		}

		[Xunit.Fact]
		public async Task EnqueueAsyncTest()
		{
			var queue = (MessageQueue)_provider.GetQueue(EXISTS_QUEUE_NAME);
			Assert.NotNull(queue);

			await queue.EnqueueAsync("Are you OK?!");
			await queue.EnqueueAsync("Are you OK?! (with 60 seconds)", new Zongsoft.Messaging.MessageEnqueueSettings(TimeSpan.FromSeconds(60)));
		}

		[Xunit.Fact]
		public async Task DequeueAndDeleteAsyncTest()
		{
			var queue = (MessageQueue)_provider.GetQueue(EXISTS_QUEUE_NAME);
			Assert.NotNull(queue);

			var message = await queue.DequeueAsync();

			if(message != null)
				await message.AcknowledgeAsync(null);
		}

		[Xunit.Fact]
		public async Task DequeueAndDelayAsyncTest()
		{
			var queue = (MessageQueue)_provider.GetQueue(EXISTS_QUEUE_NAME);
			Assert.NotNull(queue);

			var message = await queue.DequeueAsync();

			if(message != null)
				await message.AcknowledgeAsync(60);
		}

		[Xunit.Fact]
		public async Task PeekAsyncTest()
		{
			var queue = (MessageQueue)_provider.GetQueue(EXISTS_QUEUE_NAME);
			Assert.NotNull(queue);

			var message = await queue.PeekAsync();

			if(message != null)
				Assert.NotNull(message.Id);
		}
		#endregion
	}
}
