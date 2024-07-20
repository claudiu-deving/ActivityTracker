using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientTests.Support;

public class Labeled<T>
{
	public Labeled(T data, string label)
	{
		Data = data;
		Label = label;
	}

	public T Data { get; }
	public string Label { get; }

	public override string ToString()
	{
		return Label;
	}
}

public static class Labeledextensions
{
	public static Labeled<T> Labeled<T>
			(this T source, string label) => new Labeled<T>(source, label);
}
