/*
 * Authors:
 *   钟峰(Popeye Zhong) <zongsoft@gmail.com>
 *
 * Copyright (C) 2015-2016 Zongsoft Corporation <http://www.zongsoft.com>
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
using System.Collections.Concurrent;

namespace Zongsoft.Externals.Aliyun.Messaging
{
	public class MessageQueueProvider : Zongsoft.Collections.IQueueProvider
	{
		#region 成员字段
		private ConcurrentDictionary<string, MessageQueue> _queues;
		private Options.IConfiguration _configuration;
		#endregion

		#region 构造函数
		public MessageQueueProvider()
		{
			_queues = new ConcurrentDictionary<string, MessageQueue>(StringComparer.OrdinalIgnoreCase);
		}

		public MessageQueueProvider(Options.Configuration.GeneralConfiguration option)
		{
			if(option == null)
				throw new ArgumentNullException("option");

			_configuration = option;
			_queues = new ConcurrentDictionary<string, MessageQueue>(StringComparer.OrdinalIgnoreCase);
		}
		#endregion

		#region 公共属性
		public ICertification Certification
		{
			get
			{
				return _configuration.Certification;
			}
		}

		public Options.IConfiguration Configuration
		{
			get
			{
				return _configuration;
			}
			set
			{
				if(value == null)
					throw new ArgumentNullException();

				_configuration = value;
			}
		}

		public MessageQueueServiceCenter ServiceCenter
		{
			get
			{
				return MessageQueueServiceCenter.GetInstance(_configuration.Name, _configuration.IsInternal);
			}
		}

		public MessageQueue this[string name]
		{
			get
			{
				if(string.IsNullOrWhiteSpace(name))
					throw new ArgumentNullException("name");

				return _queues.GetOrAdd(name, key => new MessageQueue(this, name));
			}
		}
		#endregion

		#region 公共方法
		public Collections.IQueue GetQueue(string name)
		{
			if(string.IsNullOrWhiteSpace(name))
				throw new ArgumentNullException("name");

			return _queues.GetOrAdd(name, key => new MessageQueue(this, name));
		}
		#endregion

		#region	内部方法
		internal string GetRequestUrl(params string[] parts)
		{
			var configuration = this.Configuration;

			if(configuration == null)
				throw new InvalidOperationException("Missing required configuration.");

			var path = parts == null ? string.Empty : string.Join("/", parts);

			if(string.IsNullOrEmpty(path))
				return string.Format("http://{0}.{1}/queues", configuration.Messaging.Name, this.ServiceCenter.Path);
			else
				return string.Format("http://{0}.{1}/queues/{2}", configuration.Messaging.Name, this.ServiceCenter.Path, path);
		}
		#endregion
	}
}
