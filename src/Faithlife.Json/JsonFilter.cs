using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Faithlife.Utility;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Faithlife.Json
{
	/// <summary>
	/// Filters data from JSON.
	/// </summary>
	public sealed class JsonFilter
	{
		/// <summary>
		/// Attempts to create a filter from a string.
		/// </summary>
		/// <param name="value">The string.</param>
		/// <returns>The corresponding filter, or null.</returns>
		/// <remarks>A filter string is one or more JSON paths separated by commas.
		/// Each JSON path is one or more property names separated by periods.
		/// A JSON path prefixed with an exclamation point is excluded rather than included.
		/// If no JSON paths are found, this method returns null.</remarks>
		public static JsonFilter TryParse(string value)
		{
			return TryParse(value, null);
		}

		/// <summary>
		/// Attempts to create a filter from a string.
		/// </summary>
		/// <param name="value">The string.</param>
		/// <param name="rootPath">The root path.</param>
		/// <returns>The corresponding filter, or null.</returns>
		/// <remarks>This overload automatically prefixes each JSON path
		/// with the specified root path, if specified.</remarks>
		public static JsonFilter TryParse(string value, string rootPath)
		{
			if (value.IsNullOrWhiteSpace())
				return Empty;

			var paths = new List<PropertyPath>();

			if (!string.IsNullOrEmpty(rootPath))
			{
				if (rootPath.IndexOfAny(s_pathSeparators) != -1)
					return null;

				PropertyPath parsedRootPath = PropertyPath.TryParse(rootPath, null);
				if (parsedRootPath == null || parsedRootPath.IsExcluded)
					return null;

				string nextPrefix = "";
				foreach (var rootPathPart in parsedRootPath.Parts)
				{
					paths.Add(PropertyPath.TryParse(nextPrefix + AnyProperty, null));
					nextPrefix += rootPathPart + PropertySeparator;
				}
			}

			paths.AddRange(SplitFullPaths(value)
				.Select(x => PropertyPath.TryParse(x, rootPath)));

			if (paths.Any(x => x == null))
				return null;

			FilterNode rootNode = new FilterNode();
			foreach (PropertyPath path in paths.WhereNotNull())
				rootNode.AddPath(path);
			return new JsonFilter(rootNode);
		}

		/// <summary>
		/// Creates a filter from a string.
		/// </summary>
		/// <remarks>Like TryParse, but throws FormatException on failure.</remarks>
		public static JsonFilter Parse(string value)
		{
			return Parse(value, null);
		}

		/// <summary>
		/// Creates a filter from a string.
		/// </summary>
		/// <remarks>Like TryParse, but throws FormatException on failure.</remarks>
		public static JsonFilter Parse(string value, string rootPath)
		{
			JsonFilter filter = TryParse(value, rootPath);
			if (filter == null)
				throw new FormatException("Invalid filter syntax.");
			return filter;
		}

		/// <summary>
		/// Creates a JSON writer that filters JSON as it is written to the wrapped JSON writer.
		/// </summary>
		/// <param name="writer">The wrapped JSON writer.</param>
		/// <returns>The filtered JSON writer.</returns>
		public JsonWriter CreateFilteredJsonWriter(JsonWriter writer)
		{
			if (writer == null)
				throw new ArgumentNullException("writer");

			return new FilteredJsonWriter(writer, m_rootNode);
		}

		/// <summary>
		/// Filters data from the specified token.
		/// </summary>
		/// <param name="token">The input token.</param>
		/// <returns>The output token.</returns>
		public JToken FilterToken(JToken token)
		{
			if (token == null)
				return null;

			var tokenWriter = new JTokenWriter();
			token.WriteTo(CreateFilteredJsonWriter(tokenWriter));
			return tokenWriter.Token;
		}

		/// <summary>
		/// Filters data from the specified object.
		/// </summary>
		/// <typeparam name="T">The type of the object.</typeparam>
		/// <param name="value">The object instance.</param>
		/// <returns>A new object with the specified data filtered.</returns>
		/// <remarks>This method converts the object to JSON, filtering out data as
		/// appropriate, and then converts the filtered JSON back into an object.</remarks>
		public T FilterObject<T>(T value)
		{
			return FilterObject(value, null, null);
		}

		/// <summary>
		/// Filters data from the specified object.
		/// </summary>
		/// <typeparam name="T">The type of the object.</typeparam>
		/// <param name="value">The object instance.</param>
		/// <param name="outputSettings">The settings used when converting the object to JSON.</param>
		/// <param name="inputSettings">The settings used when converting the filtered JSON back to an object.</param>
		/// <returns>A new object with the specified data filtered.</returns>
		/// <remarks>This method converts the object to JSON, filtering out data as
		/// appropriate, and then converts the filtered JSON back into an object.</remarks>
		public T FilterObject<T>(T value, JsonOutputSettings outputSettings, JsonInputSettings inputSettings)
		{
			if (value == null)
				return default(T);

			var tokenWriter = new JTokenWriter();
			JsonUtility.ToJsonWriter(value, outputSettings, CreateFilteredJsonWriter(tokenWriter));
			return JsonUtility.FromJToken<T>(tokenWriter.Token, inputSettings);
		}

		/// <summary>
		/// Determines if the specified path is included by the filter.
		/// </summary>
		/// <param name="path">The path.</param>
		/// <returns>True if the specified path is included by the filter.</returns>
		public bool IsPathIncluded(string path)
		{
			PropertyPath propertyPath = PropertyPath.TryParse(path, null);
			if (propertyPath == null || propertyPath.IsExcluded)
				return false;

			FilterNode node = m_rootNode;
			foreach (string part in propertyPath.Parts)
			{
				FilterNode childNode = node.FindChild(part);
				if (!ShouldIncludeProperty(node, childNode))
					return false;
				if (childNode == null)
					break;
				node = childNode;
			}

			return true;
		}

		/// <summary>
		/// Gets all of the property paths.
		/// </summary>
		/// <remarks>Excluded paths start with an ExcludePrefix.</remarks>
		public ReadOnlyCollection<string> GetPropertyPaths()
		{
			return m_rootNode.RenderChildren("").ToList().AsReadOnly();
		}

		/// <summary>
		/// Converts the filter to a parsable string.
		/// </summary>
		public override string ToString()
		{
			return JoinPaths(GetPropertyPaths());
		}

		/// <summary>
		/// An empty JSON filter.
		/// </summary>
		public static JsonFilter Empty = new JsonFilter(new FilterNode());

		/// <summary>
		/// Character used to separate properties in a path.
		/// </summary>
		public const char PropertySeparator = '.';

		/// <summary>
		/// Character used to exclude a path.
		/// </summary>
		public const char ExcludePrefix = '!';

		/// <summary>
		/// Character used to separate paths.
		/// </summary>
		public const char PathSeparator = ',';

		/// <summary>
		/// Alternate character used to separate paths.
		/// </summary>
		public const char AlternatePathSepartor = ';';

		/// <summary>
		/// Character used to open a group.
		/// </summary>
		public const char GroupOpener = '(';

		/// <summary>
		/// Character used to close a group.
		/// </summary>
		public const char GroupCloser = ')';

		/// <summary>
		/// String used to indicate any property.
		/// </summary>
		public const string AnyProperty = "*";

		/// <summary>
		/// Determines if the specified property is included by the filter.
		/// </summary>
		/// <param name="expression">The property expression.</param>
		/// <returns>True if the specified property is included by the filter.</returns>
		public bool IsPropertyIncluded<TOwner>(Expression<Func<TOwner, object>> expression)
		{
			return IsPathIncluded(GetPropertyPath(expression));
		}

		/// <summary>
		/// Excludes the specified path with the standard prefix.
		/// </summary>
		/// <param name="path">The path.</param>
		/// <returns>The excluded path.</returns>
		public static string ExcludePath(string path)
		{
			if (path == null)
				throw new ArgumentNullException("path");

			return ExcludePrefix + path;
		}

		/// <summary>
		/// Joins the specified paths with the standard delimiter.
		/// </summary>
		/// <param name="paths">The paths.</param>
		/// <returns>The joined paths.</returns>
		public static string JoinPaths(IEnumerable<string> paths)
		{
			if (paths == null)
				throw new ArgumentNullException("paths");

			return paths.Join(PathSeparator);
		}

		/// <summary>
		/// Joins the specified paths with the standard delimiter.
		/// </summary>
		/// <param name="paths">The paths.</param>
		/// <returns>The joined paths.</returns>
		public static string JoinPaths(params string[] paths)
		{
			return JoinPaths((IEnumerable<string>) paths);
		}

		/// <summary>
		/// Gets the path of the specified property.
		/// </summary>
		/// <param name="expression">The property expression.</param>
		/// <returns>The path of the specified property.</returns>
		/// <remarks>The returned path is always lowercase.</remarks>
		public static string GetPropertyPath<TOwner>(Expression<Func<TOwner, object>> expression)
		{
			if (expression == null)
				throw new ArgumentNullException("expression");

			string propertyPath = DoGetPropertyPath(expression.Body);
			if (propertyPath == null)
				throw new ArgumentException("Could not determine property path for " + expression, "expression");
			return propertyPath;
		}

		/// <summary>
		/// Gets the excluded path of the specified property.
		/// </summary>
		/// <param name="expression">The property expression.</param>
		/// <returns>The excluded path of the specified property.</returns>
		/// <remarks>The returned path is always lowercase.</remarks>
		public static string GetExcludedPropertyPath<TOwner>(Expression<Func<TOwner, object>> expression)
		{
			return ExcludePath(GetPropertyPath(expression));
		}

		/// <summary>
		/// Joins the paths of the specified properties with the standard delimiter.
		/// </summary>
		/// <param name="expressions">The property expressions.</param>
		/// <returns>The joined paths of the specified properties.</returns>
		/// <remarks>The returned paths are always lowercase.</remarks>
		public static string JoinPropertyPaths<TOwner>(params Expression<Func<TOwner, object>>[] expressions)
		{
			if (expressions == null)
				throw new ArgumentNullException("expressions");

			return JoinPaths(expressions.Select(GetPropertyPath));
		}

		/// <summary>
		/// Joins the excluded paths of the specified properties with the standard delimiter.
		/// </summary>
		/// <param name="expressions">The property expressions.</param>
		/// <returns>The joined paths of the specified properties.</returns>
		/// <remarks>The returned paths are always lowercase.</remarks>
		public static string JoinExcludedPropertyPaths<TOwner>(params Expression<Func<TOwner, object>>[] expressions)
		{
			if (expressions == null)
				throw new ArgumentNullException("expressions");

			return JoinPaths(expressions.Select(GetExcludedPropertyPath));
		}

		private JsonFilter(FilterNode rootNode)
		{
			m_rootNode = rootNode;
		}

		private static bool ShouldIncludeProperty(FilterNode node, FilterNode childNode)
		{
			if (childNode != null)
			{
				// if this property or any children of this property are included, include it
				if (childNode.IsAnyIncluded())
					return true;

				// if this property is explicitly excluded, exclude it
				if (childNode.IsIncluded == false)
					return false;
			}

			// exclude this property if any siblings are included
			return !node.IsSiblingIncluded(childNode);
		}

		private static string DoGetPropertyPath(Expression expression)
		{
			// pass through method calls (most importantly collection indexers)
			MethodCallExpression methodCallExpression = expression as MethodCallExpression;
			if (methodCallExpression != null)
				return DoGetPropertyPath(methodCallExpression.Object);

			// pass through unary expressions (most importantly casts)
			UnaryExpression unaryExpression = expression as UnaryExpression;
			if (unaryExpression != null)
				return DoGetPropertyPath(unaryExpression.Operand);

			// detect property name
			MemberExpression memberExpression = expression as MemberExpression;
			if (memberExpression != null)
			{
				// get property name; use JsonPropertyAttribute if present
				MemberInfo member = memberExpression.Member;
				var attribute = member.GetCustomAttribute<JsonPropertyAttribute>();
				string name = attribute == null || string.IsNullOrEmpty(attribute.PropertyName) ? member.Name : attribute.PropertyName;

				// check for parent path
				string parentPath = null;
				if (memberExpression.Expression != null)
					parentPath = DoGetPropertyPath(memberExpression.Expression);
				return (parentPath == null ? "" : (parentPath + PropertySeparator)) + name.ToLowerInvariant();
			}

			return null;
		}

		private static IEnumerable<string> SplitFullPaths(string text)
		{
			int index = 0;
			return SplitFullPaths(text, ref index);
		}

		private static IEnumerable<string> SplitFullPaths(string text, ref int index)
		{
			List<string> results = new List<string>();

			IEnumerable<string> prefixes = null;
			while (true)
			{
				int nextIndex = index < text.Length ? text.IndexOfAny(s_pathSeparators, index) : -1;
				if (nextIndex == -1)
					nextIndex = text.Length;

				string result = text.Substring(index, nextIndex - index).Trim();
				if (result.Length != 0)
					prefixes = prefixes == null ? new List<string> { result } : prefixes.Select(prefix => prefix + result);

				char nextChar = nextIndex == text.Length ? default(char) : text[nextIndex];
				if (nextChar == GroupOpener)
				{
					index = nextIndex + 1;
					IEnumerable<string> groupPaths = SplitFullPaths(text, ref index);
					prefixes = prefixes == null ? groupPaths : prefixes.SelectMany(prefix => groupPaths.Select(groupPath => prefix + groupPath));
				}
				else
				{
					if (prefixes != null)
						results.AddRange(prefixes);
					prefixes = null;

					index = nextIndex;
					if (nextChar == default(char) || nextChar == GroupCloser)
						break;
				}

				if (index < text.Length)
					index++;
			}

			return results.AsReadOnly();
		}

		private sealed class PropertyPath
		{
			public PropertyPath(IEnumerable<string> parts, bool isExcluded)
			{
				m_parts = parts.ToList().AsReadOnly();
				m_isExcluded = isExcluded;
			}

			public static PropertyPath TryParse(string fullName, string rootPath)
			{
				string prefix = rootPath.IsNullOrWhiteSpace() ? "" : (rootPath.Trim() + PropertySeparator);

				var parts = (prefix + fullName).Split(new[] { PropertySeparator }, StringSplitOptions.None).Select(x => x.Trim()).ToList();
				bool isExcluded = false;

				for (int index = 0; index < parts.Count; index++)
				{
					string part = parts[index];
					if (part.Length == 0)
						return null;

					while (part[0] == ExcludePrefix)
					{
						part = part.Substring(1).Trim();
						if (part.Length == 0)
							return null;

						parts[index] = part;
						isExcluded = !isExcluded;
					}
				}

				return parts.Count == 0 ? null : new PropertyPath(parts, isExcluded);
			}

			public ReadOnlyCollection<string> Parts
			{
				get { return m_parts; }
			}

			public bool IsExcluded
			{
				get { return m_isExcluded; }
			}

			readonly ReadOnlyCollection<string> m_parts;
			readonly bool m_isExcluded;
		}

		private sealed class FilterNode
		{
			public FilterNode()
			{
				m_children = new Dictionary<string, FilterNode>(StringComparer.OrdinalIgnoreCase);
			}

			public bool? IsIncluded
			{
				get { return m_isIncluded; }
			}

			public FilterNode FindChild(string name)
			{
				return m_children.GetValueOrDefault(name) ?? m_children.GetValueOrDefault(AnyProperty);
			}

			public bool IsAnyIncluded()
			{
				// look for any included descendant
				return m_isIncluded == true || m_children.Values.Any(x => x.IsAnyIncluded());
			}

			public bool IsSiblingIncluded(FilterNode childNode)
			{
				var children = m_children.Values;
				if (children.Any(x => x != childNode && x.IsIncluded == true))
					return true;
				else if (children.Any(x => x != childNode && x.IsIncluded == false))
					return false;
				else
					return children.Any(x => x != childNode && x.IsSiblingIncluded(null));
			}

			public void AddPath(PropertyPath path)
			{
				FilterNode childNode = m_children.GetOrAddValue(path.Parts[0], () => new FilterNode());
				if (path.Parts.Count == 1)
				{
					bool isIncluded = !path.IsExcluded;
					if (childNode.m_isIncluded == null)
						childNode.m_isIncluded = isIncluded;
					else if (childNode.m_isIncluded != isIncluded)
						childNode.m_isIncluded = null;
				}
				else
				{
					childNode.AddPath(new PropertyPath(path.Parts.Skip(1), path.IsExcluded));
				}
			}

			public IEnumerable<string> RenderChildren(string prefix)
			{
				foreach (var childNode in m_children.OrderBy(x => x.Key))
				{
					string fullName = prefix + childNode.Key;

					bool? isIncluded = childNode.Value.IsIncluded;
					if (isIncluded != null)
						yield return isIncluded == false ? ExcludePrefix + fullName : fullName;

					foreach (string descendant in childNode.Value.RenderChildren(fullName + PropertySeparator))
						yield return descendant;
				}
			}

			readonly Dictionary<string, FilterNode> m_children;
			bool? m_isIncluded;
		}

		private sealed class FilteredJsonWriter : JsonWriter
		{
			public FilteredJsonWriter(JsonWriter writer, FilterNode node)
			{
				m_writer = writer;
				m_statusStack = new Stack<Status>();
				m_statusStack.Push(new Status { IsIncluded = true, Node = node });
			}

			public override void Flush()
			{
				m_writer.Flush();
			}

			public override void Close()
			{
				// Here and elsewhere, we check to see if we are being called by a client or
				// by the implementation of the standard JsonWriter.
				m_reentrancy++;
				base.Close();
				m_reentrancy--;

				if (m_reentrancy == 0)
				{
					// if CloseOutput is false, don't close output on wrapped writer
					bool closeOutputOverridden = !CloseOutput && m_writer.CloseOutput;
					if (closeOutputOverridden)
						m_writer.CloseOutput = false;
					try
					{
						m_writer.Close();
					}
					finally
					{
						if (closeOutputOverridden)
							m_writer.CloseOutput = true;
					}
				}
			}

			public override void WriteStartObject()
			{
				m_reentrancy++;
				base.WriteStartObject();
				m_reentrancy--;

				if (ShouldWriteStart())
					m_writer.WriteStartObject();
			}

			public override void WriteEndObject()
			{
				m_reentrancy++;
				base.WriteEndObject();
				m_reentrancy--;

				if (ShouldWriteEnd())
					m_writer.WriteEndObject();
			}

			public override void WriteStartArray()
			{
				m_reentrancy++;
				base.WriteStartArray();
				m_reentrancy--;

				if (ShouldWriteStart())
					m_writer.WriteStartArray();
			}

			public override void WriteEndArray()
			{
				m_reentrancy++;
				base.WriteEndArray();
				m_reentrancy--;

				if (ShouldWriteEnd())
					m_writer.WriteEndArray();
			}

			public override void WriteStartConstructor(string name)
			{
				m_reentrancy++;
				base.WriteStartConstructor(name);
				m_reentrancy--;

				if (ShouldWriteStart())
					m_writer.WriteStartConstructor(name);
			}

			public override void WriteEndConstructor()
			{
				m_reentrancy++;
				base.WriteEndConstructor();
				m_reentrancy--;

				if (ShouldWriteEnd())
					m_writer.WriteEndConstructor();
			}

			public override void WritePropertyName(string name)
			{
				m_reentrancy++;
				base.WritePropertyName(name);
				m_reentrancy--;

				if (m_reentrancy == 0)
				{
					// pop the previous property off the stack if we aren't the first one
					if (m_statusStack.Peek().IsProperty)
						m_statusStack.Pop();

					// check to see if this property has been included, or an ancestor or a descendant
					Status status = m_statusStack.Peek();
					FilterNode node = status.Node;
					FilterNode childNode = node == null ? null : node.FindChild(name);
					bool isIncluded = status.IsIncluded && (node == null || ShouldIncludeProperty(node, childNode));
					if (isIncluded)
						m_writer.WritePropertyName(name);

					// push a node for this property onto the stack
					m_statusStack.Push(new Status { IsIncluded = isIncluded, IsProperty = true, Node = childNode });
				}
			}

			public override void WriteEnd()
			{
				m_reentrancy++;
				base.WriteEnd();
				m_reentrancy--;

				if (ShouldWriteEnd())
					m_writer.WriteEnd();
			}

			public override void WriteNull()
			{
				m_reentrancy++;
				base.WriteNull();
				m_reentrancy--;

				if (ShouldWriteValue())
					m_writer.WriteNull();
			}

			public override void WriteUndefined()
			{
				m_reentrancy++;
				base.WriteUndefined();
				m_reentrancy--;

				if (ShouldWriteValue())
					m_writer.WriteUndefined();
			}

			public override void WriteRaw(string json)
			{
				m_reentrancy++;
				base.WriteRaw(json);
				m_reentrancy--;

				if (ShouldWriteValue())
					m_writer.WriteRaw(json);
			}

			public override void WriteRawValue(string json)
			{
				m_reentrancy++;
				base.WriteRawValue(json);
				m_reentrancy--;

				if (ShouldWriteValue())
					m_writer.WriteRawValue(json);
			}

			public override void WriteValue(string value)
			{
				m_reentrancy++;
				base.WriteValue(value);
				m_reentrancy--;

				if (ShouldWriteValue())
					m_writer.WriteValue(value);
			}

			public override void WriteValue(int value)
			{
				m_reentrancy++;
				base.WriteValue(value);
				m_reentrancy--;

				if (ShouldWriteValue())
					m_writer.WriteValue(value);
			}

			public override void WriteValue(uint value)
			{
				m_reentrancy++;
				base.WriteValue(value);
				m_reentrancy--;

				if (ShouldWriteValue())
					m_writer.WriteValue(value);
			}

			public override void WriteValue(long value)
			{
				m_reentrancy++;
				base.WriteValue(value);
				m_reentrancy--;

				if (ShouldWriteValue())
					m_writer.WriteValue(value);
			}

			public override void WriteValue(ulong value)
			{
				m_reentrancy++;
				base.WriteValue(value);
				m_reentrancy--;

				if (ShouldWriteValue())
					m_writer.WriteValue(value);
			}

			public override void WriteValue(float value)
			{
				m_reentrancy++;
				base.WriteValue(value);
				m_reentrancy--;

				if (ShouldWriteValue())
					m_writer.WriteValue(value);
			}

			public override void WriteValue(double value)
			{
				m_reentrancy++;
				base.WriteValue(value);
				m_reentrancy--;

				if (ShouldWriteValue())
					m_writer.WriteValue(value);
			}

			public override void WriteValue(bool value)
			{
				m_reentrancy++;
				base.WriteValue(value);
				m_reentrancy--;

				if (ShouldWriteValue())
					m_writer.WriteValue(value);
			}

			public override void WriteValue(short value)
			{
				m_reentrancy++;
				base.WriteValue(value);
				m_reentrancy--;

				if (ShouldWriteValue())
					m_writer.WriteValue(value);
			}

			public override void WriteValue(ushort value)
			{
				m_reentrancy++;
				base.WriteValue(value);
				m_reentrancy--;

				if (ShouldWriteValue())
					m_writer.WriteValue(value);
			}

			public override void WriteValue(char value)
			{
				m_reentrancy++;
				base.WriteValue(value);
				m_reentrancy--;

				if (ShouldWriteValue())
					m_writer.WriteValue(value);
			}

			public override void WriteValue(byte value)
			{
				m_reentrancy++;
				base.WriteValue(value);
				m_reentrancy--;

				if (ShouldWriteValue())
					m_writer.WriteValue(value);
			}

			public override void WriteValue(sbyte value)
			{
				m_reentrancy++;
				base.WriteValue(value);
				m_reentrancy--;

				if (ShouldWriteValue())
					m_writer.WriteValue(value);
			}

			public override void WriteValue(decimal value)
			{
				m_reentrancy++;
				base.WriteValue(value);
				m_reentrancy--;

				if (ShouldWriteValue())
					m_writer.WriteValue(value);
			}

			public override void WriteValue(DateTime value)
			{
				m_reentrancy++;
				base.WriteValue(value);
				m_reentrancy--;

				if (ShouldWriteValue())
					m_writer.WriteValue(value);
			}

			public override void WriteValue(DateTimeOffset value)
			{
				m_reentrancy++;
				base.WriteValue(value);
				m_reentrancy--;

				if (ShouldWriteValue())
					m_writer.WriteValue(value);
			}

			public override void WriteValue(Guid value)
			{
				m_reentrancy++;
				base.WriteValue(value);
				m_reentrancy--;

				if (ShouldWriteValue())
					m_writer.WriteValue(value);
			}

			public override void WriteValue(TimeSpan value)
			{
				m_reentrancy++;
				base.WriteValue(value);
				m_reentrancy--;

				if (ShouldWriteValue())
					m_writer.WriteValue(value);
			}

			public override void WriteValue(int? value)
			{
				m_reentrancy++;
				base.WriteValue(value);
				m_reentrancy--;

				if (ShouldWriteValue())
					m_writer.WriteValue(value);
			}

			public override void WriteValue(uint? value)
			{
				m_reentrancy++;
				base.WriteValue(value);
				m_reentrancy--;

				if (ShouldWriteValue())
					m_writer.WriteValue(value);
			}

			public override void WriteValue(long? value)
			{
				m_reentrancy++;
				base.WriteValue(value);
				m_reentrancy--;

				if (ShouldWriteValue())
					m_writer.WriteValue(value);
			}

			public override void WriteValue(ulong? value)
			{
				m_reentrancy++;
				base.WriteValue(value);
				m_reentrancy--;

				if (ShouldWriteValue())
					m_writer.WriteValue(value);
			}

			public override void WriteValue(float? value)
			{
				m_reentrancy++;
				base.WriteValue(value);
				m_reentrancy--;

				if (ShouldWriteValue())
					m_writer.WriteValue(value);
			}

			public override void WriteValue(double? value)
			{
				m_reentrancy++;
				base.WriteValue(value);
				m_reentrancy--;

				if (ShouldWriteValue())
					m_writer.WriteValue(value);
			}

			public override void WriteValue(bool? value)
			{
				m_reentrancy++;
				base.WriteValue(value);
				m_reentrancy--;

				if (ShouldWriteValue())
					m_writer.WriteValue(value);
			}

			public override void WriteValue(short? value)
			{
				m_reentrancy++;
				base.WriteValue(value);
				m_reentrancy--;

				if (ShouldWriteValue())
					m_writer.WriteValue(value);
			}

			public override void WriteValue(ushort? value)
			{
				m_reentrancy++;
				base.WriteValue(value);
				m_reentrancy--;

				if (ShouldWriteValue())
					m_writer.WriteValue(value);
			}

			public override void WriteValue(char? value)
			{
				m_reentrancy++;
				base.WriteValue(value);
				m_reentrancy--;

				if (ShouldWriteValue())
					m_writer.WriteValue(value);
			}

			public override void WriteValue(byte? value)
			{
				m_reentrancy++;
				base.WriteValue(value);
				m_reentrancy--;

				if (ShouldWriteValue())
					m_writer.WriteValue(value);
			}

			public override void WriteValue(sbyte? value)
			{
				m_reentrancy++;
				base.WriteValue(value);
				m_reentrancy--;

				if (ShouldWriteValue())
					m_writer.WriteValue(value);
			}

			public override void WriteValue(decimal? value)
			{
				m_reentrancy++;
				base.WriteValue(value);
				m_reentrancy--;

				if (ShouldWriteValue())
					m_writer.WriteValue(value);
			}

			public override void WriteValue(DateTime? value)
			{
				m_reentrancy++;
				base.WriteValue(value);
				m_reentrancy--;

				if (ShouldWriteValue())
					m_writer.WriteValue(value);
			}

			public override void WriteValue(DateTimeOffset? value)
			{
				m_reentrancy++;
				base.WriteValue(value);
				m_reentrancy--;

				if (ShouldWriteValue())
					m_writer.WriteValue(value);
			}

			public override void WriteValue(Guid? value)
			{
				m_reentrancy++;
				base.WriteValue(value);
				m_reentrancy--;

				if (ShouldWriteValue())
					m_writer.WriteValue(value);
			}

			public override void WriteValue(TimeSpan? value)
			{
				m_reentrancy++;
				base.WriteValue(value);
				m_reentrancy--;

				if (ShouldWriteValue())
					m_writer.WriteValue(value);
			}

			public override void WriteValue(byte[] value)
			{
				m_reentrancy++;
				base.WriteValue(value);
				m_reentrancy--;

				if (ShouldWriteValue())
					m_writer.WriteValue(value);
			}

			public override void WriteValue(Uri value)
			{
				m_reentrancy++;
				base.WriteValue(value);
				m_reentrancy--;

				if (ShouldWriteValue())
					m_writer.WriteValue(value);
			}

			public override void WriteValue(object value)
			{
				m_reentrancy++;
				base.WriteValue(value);
				m_reentrancy--;

				if (ShouldWriteValue())
					m_writer.WriteValue(value);
			}

			public override void WriteComment(string text)
			{
				m_reentrancy++;
				base.WriteComment(text);
				m_reentrancy--;

				if (ShouldWriteValue())
					m_writer.WriteComment(text);
			}

			public override void WriteWhitespace(string ws)
			{
				m_reentrancy++;
				base.WriteWhitespace(ws);
				m_reentrancy--;

				if (ShouldWriteValue())
					m_writer.WriteWhitespace(ws);
			}

			private bool ShouldWriteStart()
			{
				if (m_reentrancy != 0)
					return false;

				// push a non-property node to track depth
				Status status = m_statusStack.Peek();
				m_statusStack.Push(new Status { IsIncluded = status.IsIncluded, Node = status.Node });
				return status.IsIncluded;
			}

			private bool ShouldWriteEnd()
			{
				if (m_reentrancy != 0)
					return false;

				// pop the previous property off the stack if one was added
				if (m_statusStack.Peek().IsProperty)
					m_statusStack.Pop();

				// pop the non-property node that tracks depth
				return m_statusStack.Pop().IsIncluded;
			}

			private bool ShouldWriteValue()
			{
				return m_reentrancy == 0 && m_statusStack.Peek().IsIncluded;
			}

			private sealed class Status
			{
				public bool IsIncluded { get; set; }
				public bool IsProperty { get; set; }
				public FilterNode Node { get; set; }
			}

			readonly JsonWriter m_writer;
			readonly Stack<Status> m_statusStack;
			int m_reentrancy;
		}

		static readonly char[] s_pathSeparators = { PathSeparator, AlternatePathSepartor, GroupOpener, GroupCloser };

		readonly FilterNode m_rootNode;
	}
}
