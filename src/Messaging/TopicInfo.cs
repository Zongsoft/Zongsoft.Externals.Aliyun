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

namespace Zongsoft.Externals.Aliyun.Messaging
{
	/// <summary>
	/// 表示主题信息的实体类。
	/// </summary>
	public class TopicInfo
	{
		/// <summary>
		/// 获取或设置主题的名称。
		/// </summary>
		public string Name
		{
			get;
			set;
		}

		/// <summary>
		/// 获取或设置主题的创建时间。
		/// </summary>
		public DateTime CreatedTime
		{
			get;
			set;
		}

		/// <summary>
		/// 获取或设置主题的最后修改时间。
		/// </summary>
		public DateTime? ModifiedTime
		{
			get;
			set;
		}

		/// <summary>
		/// 获取或设置主题中消息的最大长度，单位：byte。
		/// </summary>
		public int MaximumMessageSize
		{
			get;
			set;
		}

		/// <summary>
		/// 获取或设置主题中消息的最大保持时长。
		/// </summary>
		public TimeSpan MessageRetentionPeriod
		{
			get;
			set;
		}

		/// <summary>
		/// 获取或设置当前主题中的消息数量。
		/// </summary>
		public int MessageCount
		{
			get;
			set;
		}

		/// <summary>
		/// 获取或设置一个值，指示主题队列是否启用了日志记录。
		/// </summary>
		public bool LoggingEnabled
		{
			get;
			set;
		}
	}
}
