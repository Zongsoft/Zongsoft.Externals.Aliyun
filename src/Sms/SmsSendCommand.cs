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

using Zongsoft.Services;
using Zongsoft.Resources;

namespace Zongsoft.Externals.Aliyun.Sms
{
	[CommandOption(KEY_TEMPLATE_OPTION, typeof(string), null, true, "Text.SmsSendCommand.Options.Template")]
	[CommandOption(KEY_PARAMETER_OPTION, typeof(string), null, false, "Text.SmsSendCommand.Options.Parameter")]
	[CommandOption(KEY_EXTRA_OPTION, typeof(string), null, false, "Text.SmsSendCommand.Options.Extra")]
	public class SmsSendCommand : CommandBase<CommandContext>
	{
		#region 常量定义
		private const string KEY_TEMPLATE_OPTION = "template";
		private const string KEY_PARAMETER_OPTION = "parameter";
		private const string KEY_EXTRA_OPTION = "extra";
		#endregion

		#region 成员字段
		private SmsSender _sender;
		#endregion

		#region 构造函数
		public SmsSendCommand() : base("Send")
		{
		}

		public SmsSendCommand(string name) : base(name)
		{
		}
		#endregion

		#region 公共属性
		public SmsSender Sender
		{
			get
			{
				return _sender;
			}
			set
			{
				if(value == null)
					throw new ArgumentNullException();

				_sender = value;
			}
		}
		#endregion

		#region 重写方法
		protected override object OnExecute(CommandContext context)
		{
			if(context.Expression.Arguments == null || context.Expression.Arguments.Length == 0)
				throw new CommandException("Missing arguments.");

			var sender = this.Sender;

			if(sender == null)
				throw new InvalidOperationException("Missing sms sender of the command.");

			var result = Utility.ExecuteTask(() => sender.SendAsync(
				context.Expression.Options.GetValue<string>(KEY_TEMPLATE_OPTION),
				context.Expression.Arguments,
				context.Parameter ?? this.GetDictionary(context.Expression.Options.GetValue<string>(KEY_PARAMETER_OPTION)),
				context.Expression.Options.GetValue<string>(KEY_EXTRA_OPTION)));

			return result;
		}
		#endregion

		#region 私有方法
		private IDictionary<string, string> GetDictionary(string text)
		{
			if(string.IsNullOrEmpty(text))
				return null;

			var parts = text.Split(',', '|');
			var dictionary = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

			foreach(var part in parts)
			{
				if(string.IsNullOrEmpty(part))
					continue;

				var index = part.IndexOf('=');

				if(index < 0)
					index = part.IndexOf(':');

				if(index <= 0)
					throw new CommandOptionValueException("parameter", text);

				var key = part.Substring(0, index);
				var value = index < part.Length - 1 ? part.Substring(index + 1) : string.Empty;

				dictionary[key] = value;
			}

			return dictionary;
		}
		#endregion
	}
}
