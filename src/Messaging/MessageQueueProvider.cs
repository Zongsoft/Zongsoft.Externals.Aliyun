/*
 * Authors:
 *   钟峰(Popeye Zhong) <zongsoft@gmail.com>
 *
 * Copyright (C) 2015 Zongsoft Corporation <http://www.zongsoft.com>
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
		private Zongsoft.Externals.Aliyun.Options.Configuration.GeneralConfiguration _option;
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

			_option = option;
			_queues = new ConcurrentDictionary<string, MessageQueue>(StringComparer.OrdinalIgnoreCase);
		}
		#endregion

		#region 公共属性
		public MessageQueueServiceCenter ServiceCenter
		{
			get
			{
				return MessageQueueServiceCenter.GetInstance(_option.Name, _option.IsInternal);
			}
		}

		public ICertification Certification
		{
			get
			{
				return _option.Certification;
			}
		}

		public Options.Configuration.GeneralConfiguration Option
		{
			get
			{
				return _option;
			}
			set
			{
				if(value == null)
					throw new ArgumentNullException();

				_option = value;
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

		public string GetRequestUrl(params string[] parts)
		{
			var path = parts == null ? string.Empty : string.Join("/", parts);

			if(string.IsNullOrEmpty(path))
				return string.Format("http://{0}.{1}", this.GetAccountName(), this.ServiceCenter.Path);
			else
				return string.Format("http://{0}.{1}/{2}", this.GetAccountName(), this.ServiceCenter.Path, path);
		}
		#endregion

		#region 私有方法
		private string GetAccountName()
		{
			if(_option == null)
				throw new InvalidProgramException();

			return _option.Messaging.Name;
		}
		#endregion
	}
}
