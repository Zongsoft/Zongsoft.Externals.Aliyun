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

namespace Zongsoft.Externals.Aliyun.Sms
{
	/// <summary>
	/// 表示短信发送后的结果类。
	/// </summary>
	public class SmsSendResult
	{
		#region 成员字段
		private string _code;
		private string _message;
		#endregion

		#region 构造函数
		public SmsSendResult()
		{
		}

		public SmsSendResult(string code, string message)
		{
			_code = code;
			_message = message;
		}
		#endregion

		#region 公共属性
		public string Code
		{
			get
			{
				return _code;
			}
		}

		public string Message
		{
			get
			{
				return _message;
			}
		}
		#endregion
	}
}
