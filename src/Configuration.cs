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

namespace Zongsoft.Externals.Aliyun
{
	/// <summary>
	/// 表示本应用的配置类。
	/// </summary>
	public static class Configuration
	{
		#region 成员字段
		private static Options.IConfiguration _instance;
		#endregion

		#region	公共属性
		/// <summary>
		/// 获取或设置本应用的配置对象。
		/// </summary>
		public static Options.IConfiguration Instance
		{
			get
			{
				if(_instance == null)
				{
					_instance = Zongsoft.Options.OptionManager.Instance.GetOptionValue("/Externals/Aliyun/General") as Options.IConfiguration;

					if(_instance == null)
						throw new InvalidOperationException("Missing required configuation of the Aliyun.");
				}

				return _instance;
			}
			set
			{
				_instance = value ?? throw new ArgumentNullException();
			}
		}
		#endregion
	}
}
