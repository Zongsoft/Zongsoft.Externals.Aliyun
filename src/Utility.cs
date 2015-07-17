﻿/*
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
using System.Threading.Tasks;
using System.Xml;

namespace Zongsoft.Externals.Aliyun
{
	internal static class Utility
	{
		public static readonly DateTime MINIMUM_EXPIRES = new DateTime(1970, 1, 1);

		/// <summary>
		/// 将本地时间转换成GMT格式的时间文本。
		/// </summary>
		/// <param name="datetime">本地时间。</param>
		/// <returns>返回被转换后的GMT格式的时间文本。</returns>
		public static string GetGmtTime(DateTime? datetime = null)
		{
			return (datetime.HasValue ? datetime.Value : DateTime.Now).ToUniversalTime().ToString("r");
		}

		public static long GetExpiresSeconds(DateTime datetime)
		{
			if(datetime < MINIMUM_EXPIRES)
				throw new ArgumentOutOfRangeException("datetime");

			return (long)(datetime - MINIMUM_EXPIRES).TotalSeconds;
		}

		public static DateTime GetExpiresTimeFromMilliseconds(string totalMilliseconds)
		{
			double number;

			if(Zongsoft.Common.Convert.TryConvertValue(totalMilliseconds, out number))
				return MINIMUM_EXPIRES.AddMilliseconds(number);
			else
				throw new ArgumentException(string.Format("Invalid '{0}' value of 'totalMilliseconds' argument.", totalMilliseconds));
		}

		public static TimeSpan? GetDuration(object parameter, DateTime baseTime)
		{
			if(parameter == null)
				return null;

			if(parameter.GetType() == typeof(TimeSpan))
				return (TimeSpan)parameter;

			TimeSpan? duration = null;

			switch(Type.GetTypeCode(parameter.GetType()))
			{
				case TypeCode.Byte:
				case TypeCode.SByte:
				case TypeCode.Single:
				case TypeCode.Double:
				case TypeCode.Decimal:
				case TypeCode.Int16:
				case TypeCode.Int32:
				case TypeCode.Int64:
				case TypeCode.UInt16:
				case TypeCode.UInt32:
				case TypeCode.UInt64:
					duration = TimeSpan.FromSeconds(Zongsoft.Common.Convert.ConvertValue<int>(parameter));
					break;
				case TypeCode.DateTime:
					duration = ((DateTime)parameter) - baseTime;
					break;
			}

			if(duration.HasValue)
				return duration.Value;

			throw new ArgumentException(string.Format("The '{0}' value of parameter is not supported.", parameter));
		}

		/// <summary>
		/// 异步包装方法：确保在Web程序中不会被异步操作的并发线程乱入。
		/// </summary>
		/// <typeparam name="T">返回值的类型。</typeparam>
		/// <param name="thunk">异步任务的委托。</param>
		/// <returns>返回以同步方式返回异步任务的执行结果。</returns>
		public static T ExecuteTask<T>(Func<Task<T>> thunk)
		{
			return Task.Run(() => ExecuteTaskDelegate(() => thunk())).Result;
		}

		private static async Task<T> ExecuteTaskDelegate<T>(Func<Task<T>> thunk)
		{
			return await thunk();
		}

		public static class Xml
		{
			public static void MoveToEndElement(XmlReader reader)
			{
				if(reader == null || reader.ReadState != ReadState.Interactive || reader.IsEmptyElement)
					return;

				if(reader.NodeType == XmlNodeType.Element)
				{
					int depth = reader.Depth;

					while(reader.Read() && reader.Depth > depth)
						;
				}
			}

			public static string ReadContentAsString(XmlReader reader)
			{
				if(reader.NodeType != XmlNodeType.Element)
					return null;

				if(reader.IsEmptyElement)
					return string.Empty;

				var depth = reader.Depth;
				string text = null;

				while(reader.Read() && reader.Depth > depth)
				{
					if(text == null && reader.NodeType == XmlNodeType.Text)
						text = reader.Value;
				}

				return text;
			}
		}
	}
}
