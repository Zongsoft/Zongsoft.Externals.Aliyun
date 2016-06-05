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
using System.ComponentModel;

namespace Zongsoft.Externals.Aliyun.Messaging
{
	/// <summary>
	/// 表示消息队列服务中心的类。
	/// </summary>
	public class MessageQueueServiceCenter : ServiceCenter
	{
		#region 常量定义
		//中国存储服务中心访问地址的前缀常量
		private const string OSS_CN_PREFIX = "mns.cn-";

		//美国存储服务中心访问地址的前缀常量
		private const string OSS_US_PREFIX = "mns.us-";
		#endregion

		#region 构造函数
		private MessageQueueServiceCenter(ServiceCenterName name, bool isInternal) : base(name, isInternal)
		{
			this.Path = OSS_CN_PREFIX + base.Path;
		}
		#endregion

		#region 静态方法
		public static MessageQueueServiceCenter GetInstance(ServiceCenterName name, bool isInternal = true)
		{
			switch(name)
			{
				case ServiceCenterName.Beijing:
					return isInternal ? Internal.Beijing : Public.Beijing;
				case ServiceCenterName.Qingdao:
					return isInternal ? Internal.Qingdao : Public.Qingdao;
				case ServiceCenterName.Hangzhou:
					return isInternal ? Internal.Hangzhou : Public.Hangzhou;
				case ServiceCenterName.Shenzhen:
					return isInternal ? Internal.Shenzhen : Public.Shenzhen;
				case ServiceCenterName.Hongkong:
					return isInternal ? Internal.Hongkong : Public.Hongkong;
			}

			throw new NotSupportedException();
		}
		#endregion

		#region 嵌套子类
		public static class Public
		{
			/// <summary>北京消息队列服务中心的外部访问地址</summary>
			public static readonly MessageQueueServiceCenter Beijing = new MessageQueueServiceCenter(ServiceCenterName.Beijing, false);

			/// <summary>青岛消息队列服务中心的外部访问地址</summary>
			public static readonly MessageQueueServiceCenter Qingdao = new MessageQueueServiceCenter(ServiceCenterName.Qingdao, false);

			/// <summary>杭州消息队列服务中心的外部访问地址</summary>
			public static readonly MessageQueueServiceCenter Hangzhou = new MessageQueueServiceCenter(ServiceCenterName.Hangzhou, false);

			/// <summary>深圳消息队列服务中心的外部访问地址</summary>
			public static readonly MessageQueueServiceCenter Shenzhen = new MessageQueueServiceCenter(ServiceCenterName.Shenzhen, false);

			/// <summary>香港消息队列服务中心的外部访问地址</summary>
			public static readonly MessageQueueServiceCenter Hongkong = new MessageQueueServiceCenter(ServiceCenterName.Hongkong, false);
		}

		public static class Internal
		{
			/// <summary>北京消息队列服务中心的内部访问地址</summary>
			public static readonly MessageQueueServiceCenter Beijing = new MessageQueueServiceCenter(ServiceCenterName.Beijing, true);

			/// <summary>青岛消息队列服务中心的内部访问地址</summary>
			public static readonly MessageQueueServiceCenter Qingdao = new MessageQueueServiceCenter(ServiceCenterName.Qingdao, true);

			/// <summary>杭州消息队列服务中心的内部访问地址</summary>
			public static readonly MessageQueueServiceCenter Hangzhou = new MessageQueueServiceCenter(ServiceCenterName.Hangzhou, true);

			/// <summary>深圳消息队列服务中心的内部访问地址</summary>
			public static readonly MessageQueueServiceCenter Shenzhen = new MessageQueueServiceCenter(ServiceCenterName.Shenzhen, true);

			/// <summary>香港消息队列服务中心的内部访问地址</summary>
			public static readonly MessageQueueServiceCenter Hongkong = new MessageQueueServiceCenter(ServiceCenterName.Hongkong, true);
		}
		#endregion
	}
}
