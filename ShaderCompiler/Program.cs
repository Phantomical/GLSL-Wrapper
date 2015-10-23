using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace ShaderCompiler
{
	using StageItem = Tuple<string, Program.ShaderStage, string>;
	using UniformItem = Tuple<ActiveUniformType, string>;

	public static class Extensions
	{
		public static string[] SplitWhere(this string arg, Predicate<char> pred)
		{
			List<string> strs = new List<string>();
			int prev = 0;
			int i = 0;

			for (i = 0; i < arg.Length; i++)
			{
				if (pred(arg[i]))
				{
					strs.Add(arg.Substring(prev, i - prev));
					prev = i;
				}
			}

			if (prev != i)
			{
				strs.Add(arg.Substring(i));
			}

			return strs.ToArray();
		}
		public static string TrimMatchingQuotes(this string arg)
		{
			if (arg[0] == '\"' && arg.Last() == '\"')
			{
				return arg.Substring(1, arg.Length - 2);
			}
			return arg;
		}
	}

	class Program
	{
		static GameWindow Window;
		static ArgInfo Info;
		static List<Tuple<ActiveAttribType, string>> Attributes = new List<Tuple<ActiveAttribType, string>>();
		static List<Tuple<ActiveUniformType, string>> Uniforms = new List<UniformItem>();

		/*
		 * Arguments
		 *	- out=[filename] : Sets the output file (The default is 'shader_file.h')
		 *	- name=[string] : Sets the name of the shader (This will be the name of the output class)
		 *	- [filename] : One of the files to compile as a shader stage (The stage of the shader is inferred from the extension)
		 *	- vert=[filename] : Compiles the file as a vertex shader
		 *	- frag=[filename] : Compiles the file as a fragment shader
		 *	- geom=[filename] : Compiles the file as a geometry shader
		 *	- tessEval=[filename] : Compiles the file as a tesselation evaluation shader
		 *	- tessControl=[filename] : Compiles the file as a tesseleation control shader
		 *	- compute=[filename] : Compiles the file as a compute shader (This option cannot be specified with any of the other file types)

		 */

		static void ParseArgs(string[] args)
		{
			Info = new ArgInfo();

			Info.Stages = new List<StageItem>();
			Info.Namespace = "Shaders";
			Info.ShaderName = "__Shader" + args.GetHashCode();
			Info.OutputFile = "shader_file.h";

			foreach (string arg in args)
			{
				if (arg.StartsWith("-") || arg.StartsWith("/"))
				{
					string option = arg.Substring(1);

					if (option.StartsWith("out="))
					{
						Info.OutputFile = option.Substring(4).TrimMatchingQuotes();
					}
					else if (option.StartsWith("name="))
					{
						Info.ShaderName = option.Substring(5).TrimMatchingQuotes();
					}
					else if (option.StartsWith("vert="))
					{
						Info.Stages.Add(new StageItem(option.Substring(5).TrimMatchingQuotes(), ShaderStage.Vertex, File.ReadAllText(option.Substring(5))));
					}
					else if (option.StartsWith("frag="))
					{
						Info.Stages.Add(new StageItem(option.Substring(5).TrimMatchingQuotes(), ShaderStage.Fragment, File.ReadAllText(option.Substring(5))));
					}
					else if (option.StartsWith("geom="))
					{
						Info.Stages.Add(new StageItem(option.Substring(5).TrimMatchingQuotes(), ShaderStage.Geometry, File.ReadAllText(option.Substring(5))));
					}
					else if (option.StartsWith("tessEval="))
					{
						Info.Stages.Add(new StageItem(option.Substring(9).TrimMatchingQuotes(), ShaderStage.TessEval, File.ReadAllText(option.Substring(9))));
					}
					else if (option.StartsWith("tessControl="))
					{
						Info.Stages.Add(new StageItem(option.Substring(12).TrimMatchingQuotes(), ShaderStage.TessControl, File.ReadAllText(option.Substring(12))));
					}
					else if (option.StartsWith("compute="))
					{
						Info.Stages.Add(new StageItem(option.Substring(8).TrimMatchingQuotes(), ShaderStage.Compute, File.ReadAllText(option.Substring(8))));
					}
					else if (option.StartsWith("namespace="))
					{
						Info.Namespace = option.Substring("namespace=".Length).TrimMatchingQuotes();
					}
					else
					{
						Console.WriteLine("Unknown argument: '" + arg + "'");
					}
				}
				else
				{
					if (arg.EndsWith(".vert", true, null))
					{
						Info.Stages.Add(new StageItem(arg, ShaderStage.Vertex, File.ReadAllText(arg)));
					}
					else if (arg.EndsWith(".frag", true, null))
					{
						Info.Stages.Add(new StageItem(arg, ShaderStage.Fragment, File.ReadAllText(arg)));
					}
					else if (arg.EndsWith(".geom", true, null))
					{
						Info.Stages.Add(new StageItem(arg, ShaderStage.Geometry, File.ReadAllText(arg)));
					}
					else if (arg.EndsWith(".tessEval", true, null))
					{
						Info.Stages.Add(new StageItem(arg, ShaderStage.TessEval, File.ReadAllText(arg)));
					}
					else if (arg.EndsWith(".tessControl", true, null))
					{
						Info.Stages.Add(new StageItem(arg, ShaderStage.TessControl, File.ReadAllText(arg)));
					}
					else if (arg.EndsWith(".compute", true, null))
					{
						Info.Stages.Add(new StageItem(arg, ShaderStage.Compute, File.ReadAllText(arg)));
					}
					else if (arg.EndsWith(".vs", true, null))
					{
						Info.Stages.Add(new StageItem(arg, ShaderStage.Vertex, File.ReadAllText(arg)));
					}
					else if (arg.EndsWith(".fs", true, null))
					{
						Info.Stages.Add(new StageItem(arg, ShaderStage.Fragment, File.ReadAllText(arg)));
					}
					else if (arg.EndsWith(".gs", true, null))
					{
						Info.Stages.Add(new StageItem(arg, ShaderStage.Geometry, File.ReadAllText(arg)));
					}
					else
					{
						Console.WriteLine("Unable to determine stage of file: '" + arg + "' argument will be ignored.");
					}
				}
			}
		}
		static void CreateContext()
		{
			//Window will stay invisible
			Window = new GameWindow(
				1080, 720, GraphicsMode.Default, "Gas Giant Test",
				GameWindowFlags.Default, DisplayDevice.Default,
				3, 2, GraphicsContextFlags.ForwardCompatible);
			Window.Visible = false;
		}
		static void DestroyContext()
		{
			Window.Dispose();
		}

		static string[] SplitCommandLine(string CommandLine)
		{
			bool inQuotes = false;
			bool isEscaping = false;

			return CommandLine.SplitWhere(c =>
			{
				if (c == '\\' && !isEscaping)
				{
					isEscaping = true;
					return false;
				}

				if (c == '\"' && !isEscaping)
				{
					inQuotes = !inQuotes;
				}

				isEscaping = false;

				return !inQuotes && Char.IsWhiteSpace(c);
			})
				.Select(arg => arg.Trim().TrimMatchingQuotes().Replace("\\\"", "\""))
				.Where(arg => !string.IsNullOrEmpty(arg))
				.ToArray();

		}
		static string StripPeriods(string str)
		{
			return Regex.Replace(str, ".", "");
		}

		struct ArgInfo
		{
			public string OutputFile;
			public string ShaderName;
			public string Namespace;

			public List<StageItem> Stages;
		}
		public enum ShaderStage
		{
			Vertex = ShaderType.VertexShader,
			Fragment = ShaderType.FragmentShader,
			Geometry = ShaderType.GeometryShader,
			TessEval = ShaderType.TessEvaluationShader,
			TessControl = ShaderType.TessControlShader,
			Compute = ShaderType.ComputeShader
		}
		public enum UniformType
		{
			@bool,
			@double,
			Vector2d,
			Vector3d,
			Vector4d,
			@float,
			Matrix2,
			Matrix2x3,
			Matrix2x4,
			Matrix3,
			Matrix3x2,
			Matrix3x4,
			Matrix4,
			Matrix4x2,
			Matrix4x3,
			Vector2,
			Vector3,
			Vector4,
			@int,
			Texture,
			@uint
		}

		static string GetEnumValue(ShaderStage stage)
		{
			switch (stage)
			{
				case ShaderStage.Vertex:
					return "GL_VERTEX_SHADER";
				case ShaderStage.Fragment:
					return "GL_FRAGMENT_SHADER";
				case ShaderStage.Geometry:
					return "GL_GEOMETRY_SHADER";
				case ShaderStage.TessEval:
					return "GL_TESS_EVALUATION_SHADER";
				case ShaderStage.TessControl:
					return "GL_TESS_CONTROL_SHADER";
				case ShaderStage.Compute:
					return "GL_COMPUTE_SHADER";
				default:
					//This shouldn't be possible, but we have to do something
					throw new Exception("Invalid shader stage type passed to program.");
			}
		}

		/// <summary>
		/// Compiles the shader and gets all the attributes and uniforms from the compiled shader object.
		/// </summary>
		/// <returns></returns>
		static bool TestCompile()
		{
			int ShaderID = 0;

			List<int> Stages = new List<int>();

			ShaderID = GL.CreateProgram();

			bool IsCompute = false;
			bool Failed = false;

			foreach(StageItem stage in Info.Stages)
			{
				IsCompute = IsCompute || stage.Item2 == ShaderStage.Compute;

				if(IsCompute && stage.Item2 != ShaderStage.Compute)
				{
					Console.WriteLine("Error: Compute shaders cannot be compiled with other shader types");
					return false;
				}

				int id = GL.CreateShader((ShaderType)stage.Item2);

				GL.ShaderSource(id, stage.Item3);
				GL.CompileShader(id);
				GL.AttachShader(ShaderID, id);

				int CompileStatus = 0;

				GL.GetShader(id, ShaderParameter.CompileStatus, out CompileStatus);

				if(CompileStatus == 0)
				{
					Console.WriteLine("Error while compiling shader. Info Log: \n" + Regex.Replace(GL.GetShaderInfoLog(id), "0\\(", stage.Item1 + "("));
					Failed = true;
				}

				Stages.Add(id);
			}

			if (Info.Stages.Count == 0)
				return false;

			if(Failed)
			{
				Console.WriteLine("Shader failed to compile. Exiting.");
				return false;
			}

			GL.LinkProgram(ShaderID);

			foreach(int shader in Stages)
			{
				GL.DetachShader(ShaderID, shader);
				GL.DeleteShader(shader);
			}

			int LinkStatus;
			GL.GetProgram(ShaderID, GetProgramParameterName.LinkStatus, out LinkStatus);
			string InfoLog = GL.GetProgramInfoLog(ShaderID);

			if(LinkStatus == 0)
			{
				Console.WriteLine("Shader failed to link. Info log: \n" + InfoLog);
				return false;
			}

			int Count = 0;

			GL.GetProgram(ShaderID, GetProgramParameterName.ActiveAttributes, out Count);

			for(int i = 0; i < Count; i++)
			{
				int size;
				int length;
				ActiveAttribType type;
				StringBuilder name = new StringBuilder(1024);

				GL.GetActiveAttrib(ShaderID, i, 1024, out length, out size, out type, name);

				Attributes.Add(new Tuple<ActiveAttribType,string>(type, name.ToString()));
			}

			GL.GetProgram(ShaderID, GetProgramParameterName.ActiveUniforms, out Count);

			for(int i = 0; i < Count; i++)
			{
				int size;
				int length;
				ActiveUniformType type;
				StringBuilder name = new StringBuilder(1024);

				GL.GetActiveUniform(ShaderID, i, 1024, out length, out size, out type, name);

				Uniforms.Add(new UniformItem(type, name.ToString()));
			}

			GL.DeleteProgram(ShaderID);

			return true;
		}
		static void WriteToFile()
		{
			List<string> Lines = new List<string>();

			/*
			 *	Naming Conventions
			 *		- Uniform Locations : Start with 'id_uniform_'
			 *		- Attribute Locations : Start with 'id_attrib_'
			 *		- Shader Source : [stage]Source
			 *		
			 *	Uniforms and Attributes are not managed by the compiler
			 */

			Lines.Add("// <auto-generated>");
			Lines.Add("//\tThis code was generated by a Tool.");
			Lines.Add("//");
			Lines.Add("//\tChanges to this file may cause incorrect behavior and will be lost if");
			Lines.Add("//\tthe code is regenerated.");
			Lines.Add("// <auto-generated>");
			Lines.Add("");

			Lines.Add("#include <string>");
			Lines.Add("#include <functional>");
			Lines.Add("");
			Lines.Add("#include \"GL\\glew.h\"");
			Lines.Add("#include \"glsl_lib.h\"");

			Lines.Add("");
			Lines.Add("namespace " + Info.Namespace);
			Lines.Add("{");

			#region static struct

			Lines.Add("\ttemplate<typename> struct _" + Info.ShaderName + "_statics");
			Lines.Add("\t{");
			Lines.Add("\tprotected:");
			Lines.Add("\t\tstatic std::size_t counter = 0;");
			Lines.Add("\t\tstatic GLuint program_id = 0;");
			Lines.Add("");

			foreach (var attrib in Attributes)
			{
				Lines.Add("\t\tstatic GLint id_attrib_" + StripPeriods(attrib.Item2) + " = -1;");
			}

			foreach (var uniform in Uniforms)
			{
				Lines.Add("\t\tstatic GLint id_uniform_" + StripPeriods(uniform.Item2) + " = -1;");
			}

			Lines.Add("\t};");

			#endregion

			Lines.Add("");

			Lines.Add("\tclass " + Info.ShaderName + " : public ::glsl_wrapper::shader, _" + Info.ShaderName + "_statics<void>");
			Lines.Add("\t{");
			Lines.Add("\tprivate:");

			#region Methods

			#region static compile


			Lines.Add("\t\tstatic void shader_compile()");
			Lines.Add("\t\t{");

			Lines.Add("\t\t\tGLint program = glCreateProgram();");

			{
				int ctr = 0;
				foreach (StageItem item in Info.Stages)
				{
					Lines.Add("\t\t\tGLuint shader_" + ctr + " = glCreateShader(" + GetEnumValue(item.Item2) + ");");
					Lines.Add("\t\t\tconst char* source_" + ctr +" = \"" + Regex.Replace(Regex.Replace(item.Item3, "( |\t)+", " "), "(\n|\r)+", "\\n") + "\";");
					Lines.Add("\t\t\tglShaderSource(shader_" + ctr + ", 1, &source_" + ctr + ", nullptr);");
					Lines.Add("\t\t\tglCompileShader(shader_" + ctr + ");");
					ctr++;
				}
			}

			Lines.Add("\t\t\tglLinkProgram(program);");

			{
				int ctr = 0;
				foreach (StageItem item in Info.Stages)
				{
					Lines.Add("\t\t\tglDetachShader(program, shader_" + ctr + ");");
					Lines.Add("\t\t\tglDeleteShader(program);");

					ctr++;
				}
			}

			Lines.Add("\t\t\tprogram_id = program;");

			{
				foreach (var attrib in Attributes)
				{
					Lines.Add("\t\t\tid_attrib_" + StripPeriods(attrib.Item2) + " = glGetAttribLocation(program_id, \"" + attrib.Item2 + "\");");
				}

				foreach (var uniform in Uniforms)
				{
					Lines.Add("\t\t\tid_uniform_" + StripPeriods(uniform.Item2) + " = glGetUniformLocation(program_id, \"" + uniform.Item2 + "\");");
				}
			}

			Lines.Add("\t\t}");
			#endregion

			Lines.Add("");
			Lines.Add("\tpublic:");

			#region use_program
			Lines.Add("\t\tvirtual void use_program()");
			Lines.Add("\t\t{");
			Lines.Add("\t\t\tglUseProgram(program_id);");
			Lines.Add("\t\t}");
			#endregion
			#region get_program_id
			Lines.Add("\t\tvirtual GLuint get_program_id()");
			Lines.Add("\t\t{");
			Lines.Add("\t\t\treturn program_id;");
			Lines.Add("\t\t}");
			#endregion
			#region get_uniform_location
			Lines.Add("\t\tvirtual GLint get_uniform_location(std::string name)");
			Lines.Add("\t\t{");

			{
				bool cond = true;
				foreach (var uniform in Uniforms)
				{
					if(cond)
					{
						cond = false;
						Lines.Add("\t\t\tif(name == \"" + uniform.Item2 + "\")");
					}
					else
					{
						Lines.Add("\t\t\telse if(name == \"" + uniform.Item2 + "\")");
					}

					Lines.Add("\t\t\t\treturn id_uniform_" + StripPeriods(uniform.Item2) + ";");
				}
			}

			Lines.Add("\t\t\treturn -1;");
			Lines.Add("\t\t}");
			#endregion
			#region get_attrib_location
			Lines.Add("\t\tvirtual GLint get_attrib_location(std::string name)");
			Lines.Add("\t\t{");

			{
				bool cond = true;
				foreach (var attrib in Attributes)
				{
					if (cond)
					{
						cond = false;
						Lines.Add("\t\t\tif(name == \"" + attrib.Item2 + "\")");
					}
					else
					{
						Lines.Add("\t\t\telse if(name == \"" + attrib.Item2 + "\")");
					}

					Lines.Add("\t\t\t\treturn id_attrib_" + StripPeriods(attrib.Item2) + ";");
				}
			}

			Lines.Add("\t\t\treturn -1;");
			Lines.Add("\t\t}");
			#endregion

			Lines.Add("");

			#region compile
			Lines.Add("\t\tvirtual void compile()");
			Lines.Add("\t\t{");
			Lines.Add("\t\t\tif(counter == 0)");
			Lines.Add("\t\t\t\tshader_compile();");
			Lines.Add("\t\t\tcounter++;");
			Lines.Add("\t\t}");
			#endregion
			#region recompile
			Lines.Add("\t\tvirtual void recompile()");
			Lines.Add("\t\t{");
			Lines.Add("\t\t\tglDeleteProgram(program_id);");
			Lines.Add("\t\t\tshader_compile();");
			Lines.Add("\t\t}");
			#endregion

			#region destructor
			Lines.Add("\t\tvirtual ~" + Info.ShaderName + "()");
			Lines.Add("\t\t{");
			Lines.Add("\t\t\tif(--counter == 0)");
			Lines.Add("\t\t\t\tglDeleteProgram(program_id);");
			Lines.Add("\t\t}");
			#endregion

			#region non virtual
			Lines.Add("\t\t" + Info.ShaderName + "() :");
			Lines.Add("\t\t\tuniform_func(::glsl_wrapper::_detail::empty_function) {  }");
			Lines.Add("\t\t" + Info.ShaderName + "(std::function<void()> func) :");
			Lines.Add("\t\t\tuniform_func(func) {  }");

			Lines.Add("");

			Lines.Add("\t\tstd::function<void()> uniform_func;");
			#endregion

			#endregion

			Lines.Add("\t};");
			Lines.Add("}");

			#region Write to File

			using (StreamWriter File = new StreamWriter(Info.OutputFile))
			{

				foreach (var line in Lines)
				{
					File.WriteLine(line);
				}

				File.Close();
			}
			#endregion
		}

		static void Main(string[] args)
		{
			try
			{
				ParseArgs(args);
				
				CreateContext();
				
				try
				{
					if(TestCompile())
					{
						WriteToFile();
					}
				}
				finally
				{
					DestroyContext();
				}
			}
			catch(Exception e)
			{
				Console.WriteLine("An exception occured: " + e.Message);
				Console.WriteLine("The program will now exit.");
			}
		}
	}
}
