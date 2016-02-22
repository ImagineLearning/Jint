using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jint.Tests.Runtime.Domain
{
	public static class PersonExtensionMethods
	{
		public static string GetNameAndAgeString(this Person person)
		{
			var str = string.Format("Name: {0} Age: {1}", person.Name, person.Age);
			return str;
		}
	}
}
