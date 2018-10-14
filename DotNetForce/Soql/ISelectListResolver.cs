using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNetForce.Common.Soql
{
	public interface ISelectListResolver
	{
		string GetFieldsList<T>();
	}
}
