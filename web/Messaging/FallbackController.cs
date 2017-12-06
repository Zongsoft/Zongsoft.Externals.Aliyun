/*
 * Authors:
 *   钟峰(Popeye Zhong) <zongsoft@gmail.com>
 *
 * Copyright (C) 2015-2017 Zongsoft Corporation <http://www.zongsoft.com>
 *
 * This file is part of Zongsoft.Externals.Aliyun.
 *
 * Zongsoft.Externals.Aliyun is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 2.1 of the License, or (at your option) any later version.
 *
 * Zongsoft.Externals.Aliyun is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
 * Lesser General Public License for more details.
 *
 * The above copyright notice and this permission notice shall be
 * included in all copies or substantial portions of the Software.
 *
 * You should have received a copy of the GNU Lesser General Public
 * License along with Zongsoft.Externals.Aliyun; if not, write to the Free Software
 * Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA 02110-1301 USA
 */

using System;
using System.Web.Http;

using Zongsoft.Messaging;

namespace Zongsoft.Externals.Aliyun.Messaging.Web
{
	public class FallbackController : ApiController
	{
		#region 成员字段
		private ITopicProvider _topics;
		#endregion

		#region 公共属性
		public ITopicProvider Topics
		{
			get
			{
				return _topics;
			}
			set
			{
				if(value == null)
					throw new ArgumentNullException();

				_topics = value;
			}
		}
		#endregion

		#region	公共方法
		public void Post(FallbackEntity entity)
		{
			var topic = _topics.Get(entity.TopicName);

			if(topic == null)
				throw new HttpResponseException(System.Net.HttpStatusCode.BadRequest);

			var executor = topic.Executor;

			if(executor == null)
				throw new HttpResponseException(System.Net.HttpStatusCode.InternalServerError);

			var message = new TopicMessage()
			{
				Identity = entity.TopicName + ":" + entity.SubscriptionName,
				MessageId = entity.MessageId,
				Data = entity.Message,
				Tags = entity.MessageTag,
				Checksum = Common.Convert.FromHexString(entity.MessageMD5),
				Timestamp = Utility.GetDateTimeFromEpoch(entity.PublishTime),
			};

			executor.Execute(message);
		}
		#endregion

		#region 嵌套结构
		public struct FallbackEntity
		{
			public string TopicOwner;
			public string TopicName;
			public string Subscriber;
			public string SubscriptionName;
			public string MessageId;
			public byte[] Message;
			public string MessageMD5;
			public string MessageTag;
			public int PublishTime;
		}
		#endregion
	}
}
