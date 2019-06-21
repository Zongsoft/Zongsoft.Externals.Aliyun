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

namespace Zongsoft.Externals.Aliyun.Telecom.Options
{
	/// <summary>
	/// 表示短信模板的配置项接口。
	/// </summary>
	public interface ITemplateOption
	{
		#region 公共属性
		/// <summary>
		/// 获取或设置模板的名称。
		/// </summary>
		string Name
		{
			get;
			set;
		}

		/// <summary>
		/// 获取或设置模板的代号。
		/// </summary>
		string Code
		{
			get;
			set;
		}

		/// <summary>
		/// 获取或设置模板的方案（即阿里云的短信签名）。
		/// </summary>
		string Scheme
		{
			get;
			set;
		}
		#endregion
	}
}
