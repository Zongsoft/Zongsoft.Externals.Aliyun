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
using System.Collections.Generic;
using System.Net.Http;

using Zongsoft.Messaging;

namespace Zongsoft.Externals.Aliyun.Messaging
{
	public class TopicSubscription : Zongsoft.Messaging.ITopicSubscription
	{
		#region 成员字段
		private Topic _topic;
		#endregion

		#region 构造函数
		public TopicSubscription(Topic topic)
		{
			if(topic == null)
				throw new ArgumentNullException(nameof(topic));

			_topic = topic;
		}
		#endregion

		#region 公共属性
		public Topic Topic
		{
			get
			{
				return _topic;
			}
		}
		#endregion

		#region 公共方法
		public Zongsoft.Messaging.TopicSubscription Get(string name)
		{
			throw new NotImplementedException();
		}

		public bool Subscribe(string name, string url, object state = null)
		{
			throw new NotImplementedException();
		}

		public bool Subscribe(string name, string url, TopicSubscriptionFallbackBehavior behavior, object state = null)
		{
			throw new NotImplementedException();
		}

		public bool Subscribe(string name, string url, string tags, object state = null)
		{
			throw new NotImplementedException();
		}

		public bool Subscribe(string name, string url, string tags, TopicSubscriptionFallbackBehavior behavior, object state = null)
		{
			throw new NotImplementedException();
		}

		public bool Unsubscribe(string name)
		{
			throw new NotImplementedException();
		}
		#endregion
	}
}
