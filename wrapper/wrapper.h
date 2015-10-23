#ifndef GLSL_LIB_
#define GLSL_LIB_H

#include <string>

#ifdef _MSC_VER
#	pragma comment(lib, "wrapper.lib")
#endif

namespace glsl_wrapper
{
	typedef unsigned int GLuint;
	typedef int GLint;
	typedef unsigned int GLenum;

#define GLSL_LIB_MATRIX_TYPE(m, n, type, prefix) struct prefix##mat##m##x##n \
	{ \
		static const std::size_t SIZE = m * n; \
		type value[SIZE]; \
		explicit prefix##mat##m##x##n(type val) \
		{ \
			for(std::size_t i = 0; i < SIZE; i++) \
				value[i] = val; \
		} \
		prefix##mat##m##x##n(type matrix[SIZE]) \
		{ \
			std::memcpy(value, matrix, SIZE * sizeof(decltype(*value))); \
		} \
		template<typename _Ty> prefix##mat##m##x##n(const _Ty& val) \
		{ \
			for(std::size_t i = 0; i < SIZE; ++i) \
				value[i] = val[i]; \
		} \
	 }
#define GLSL_LIB_VECTOR_TYPE(n, type, prefix) struct prefix##vec##n \
		{ \
		static const std::size_t SIZE = n; \
		type value[SIZE]; \
		explicit prefix##vec##n(type val) \
		{ \
			for(std::size_t i = 0; i < SIZE; ++i) \
				value[i] = val; \
		} \
		prefix##vec##n(type vals[n]) \
		{ \
			for(std::size_t i = 0; i < SIZE; ++i) \
				value[i] = vals[i]; \
		} \
		template<typename _Ty> prefix##vec##n(const _Ty& val) \
		{ \
			for(std::size_t i = 0; i < SIZE; ++i) \
				value[i] = val[i]; \
		} \
	}

#define GLSL_LIB_MATRIX_TYPES(type, prefix) \
		GLSL_LIB_MATRIX_TYPE(2, 2, type, prefix); \
		GLSL_LIB_MATRIX_TYPE(2, 3, type, prefix); \
		GLSL_LIB_MATRIX_TYPE(2, 4, type, prefix); \
		GLSL_LIB_MATRIX_TYPE(3, 2, type, prefix); \
		GLSL_LIB_MATRIX_TYPE(3, 3, type, prefix); \
		GLSL_LIB_MATRIX_TYPE(3, 4, type, prefix); \
		GLSL_LIB_MATRIX_TYPE(4, 2, type, prefix); \
		GLSL_LIB_MATRIX_TYPE(4, 3, type, prefix); \
		GLSL_LIB_MATRIX_TYPE(4, 4, type, prefix)

#define GLSL_LIB_VECTOR_TYPES(type, prefix) \
		GLSL_LIB_VECTOR_TYPE(2, type, prefix); \
		GLSL_LIB_VECTOR_TYPE(3, type, prefix); \
		GLSL_LIB_VECTOR_TYPE(4, type, prefix)

#define GLSL_LIB_MATRIX_METHOD(n, m, prefix) void uniform(GLint loc, prefix##mat##n##x##m v)
#define GLSL_LIB_MATRIX_METHODS(prefix) \
		GLSL_LIB_MATRIX_METHOD(2, 2, prefix); \
		GLSL_LIB_MATRIX_METHOD(2, 3, prefix); \
		GLSL_LIB_MATRIX_METHOD(2, 4, prefix); \
		GLSL_LIB_MATRIX_METHOD(3, 2, prefix); \
		GLSL_LIB_MATRIX_METHOD(3, 3, prefix); \
		GLSL_LIB_MATRIX_METHOD(3, 4, prefix); \
		GLSL_LIB_MATRIX_METHOD(4, 2, prefix); \
		GLSL_LIB_MATRIX_METHOD(4, 3, prefix); \
		GLSL_LIB_MATRIX_METHOD(4, 4, prefix)

	GLSL_LIB_MATRIX_TYPES(float, );
	GLSL_LIB_MATRIX_TYPES(double, d);

	GLSL_LIB_VECTOR_TYPES(float, );
	GLSL_LIB_VECTOR_TYPES(double, d);
	GLSL_LIB_VECTOR_TYPES(unsigned int, u);
	GLSL_LIB_VECTOR_TYPES(int, i);
	GLSL_LIB_VECTOR_TYPES(int, b);

	typedef mat2x2 mat2;
	typedef mat3x3 mat3;
	typedef mat4x4 mat4;

	typedef dmat2x2 dmat2;
	typedef dmat3x3 dmat3;
	typedef dmat4x4 dmat4;
	
	namespace _detail
	{
		void uniform(GLint loc, float v);
		void uniform(GLint loc, double v);
		void uniform(GLint loc, unsigned int v);
		void uniform(GLint loc, int v);
		void uniform(GLint loc, bool v);

		void uniform(GLint loc, vec2 v);
		void uniform(GLint loc, vec3 v);
		void uniform(GLint loc, vec4 v);
		void uniform(GLint loc, dvec2 v);
		void uniform(GLint loc, dvec3 v);
		void uniform(GLint loc, dvec4 v);
		void uniform(GLint loc, uvec2 v);
		void uniform(GLint loc, uvec3 v);
		void uniform(GLint loc, uvec4 v);
		void uniform(GLint loc, ivec2 v);
		void uniform(GLint loc, ivec3 v);
		void uniform(GLint loc, ivec4 v);

#pragma warning(disable:4003)
		//Float matrices
		//There is no prefix for the float matrices
		//The empty parameter is intended
		GLSL_LIB_MATRIX_METHODS( );
#pragma warning(default:4003)
		//Double matrices
		GLSL_LIB_MATRIX_METHODS(d);

		void uniform(GLint loc, bvec2 v)
		{
			uniform(loc, ivec2(v.value));
		}
		void uniform(GLint loc, bvec3 v)
		{
			uniform(loc, ivec3(v.value));
		}
		void uniform(GLint loc, bvec4 v)
		{
			uniform(loc, ivec4(v.value));
		}

		//This is used by the shader generator, don't remove it
		void empty_function() { }
	}



	struct texture
	{	
		enum texture_target
		{
			TEXTURE_1D,
			TEXTURE_2D,
			TEXTURE_3D,
			TEXTURE_1D_ARRAY,
			TEXTURE_2D_ARRAY,
			TEXTURE_RECTANGLE,
			TEXTURE_CUBE_MAP,
			TEXTURE_CUBE_MAP_ARRAY,
			TEXTURE_BUFFER,
			TEXTURE_2D_MULTISAMPLE,
			TEXTURE_2D_MULTISAMPLE_ARRAY
		};

		texture_target target;
		GLuint id;

		texture() = default;
		texture(texture_target tgt, GLuint id) :
			target(tgt),
			id(id)
		{

		}
		texture(GLenum tgt, GLuint id) :
			target(static_cast<texture_target>(tgt)),
			id(id)
		{

		}

		void bind();
	};

	class shader
	{
	private:
		static const std::string mvp_name;

	public:
		//Initializes shader variables
		virtual void use_shader() = 0;
		//Will probably become required at a later time
		virtual void pass_uniforms() { };
		
		virtual void set_mvp(mat4 matrix)
		{
			set_uniform(mvp_name, matrix);
		}

		virtual GLuint get_program_id() = 0;
		
		//Get the uniform location that is referred to by name
		virtual GLint get_uniform_location(std::string name) = 0;
		//Get the attribute that is referred to by name
		virtual GLint get_attrib_location(std::string name) = 0;

		virtual void compile() = 0;
		virtual void recompile() = 0;

		virtual ~shader();

		template<typename _Ty>
		void set_uniform(const std::string& name, const _Ty& value)
		{
			GLint uloc = get_uniform_location(name);

			if (uloc != -1)
				_detail::uniform(uloc, value);
		}

	};

#undef GLSL_LIB_MATRIX_TYPE
#undef GLSL_LIB_MATRIX_TYPES
#undef GLSL_LIB_VECTOR_TYPE
#undef GLSL_LIB_VECTOR_TYPES
#undef GLSL_LIB_MATRIX_METHOD
#undef GLSL_LIB_MATRIX_MATHODS
}

#endif
