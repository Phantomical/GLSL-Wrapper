#include "glsl_lib.h"

#include "GL\glew.h"

#define TRANSPOSE GL_FALSE

#define UNIFORM0(size, prefix) glUniform##size##prefix##v(loc, size, v.value)
#define UNIFORM1(size, prefix, vec_p) void uniform(GLint loc, vec_p##vec##size v) { UNIFORM0(size, prefix); }
#define UNIFORM(prefix, vec_p) \
	UNIFORM1(2, prefix, vec_p) \
	UNIFORM1(3, prefix, vec_p) \
	UNIFORM1(4, prefix, vec_p)

#define MAT_UNIFORM0(m, n, prefix, mat_p) void uniform(GLint loc, mat_p##mat##m##x##n v) { glUniformMatrix##m##x##n##prefix##v(loc, 1, TRANSPOSE, v.value); }
#define MAT_UNIFORM1(m, prefix, mat_p) void uniform(GLint loc, mat_p##mat##m##x##m v) { glUniformMatrix##m##prefix##v(loc, 1, TRANSPOSE, v.value); }
#define MAT_UNIFORM(prefix, mat_p) \
	MAT_UNIFORM1(2, prefix, mat_p) \
	MAT_UNIFORM1(3, prefix, mat_p) \
	MAT_UNIFORM1(4, prefix, mat_p) \
	MAT_UNIFORM0(2, 3, prefix, mat_p) \
	MAT_UNIFORM0(2, 4, prefix, mat_p) \
	MAT_UNIFORM0(3, 2, prefix, mat_p) \
	MAT_UNIFORM0(3, 4, prefix, mat_p) \
	MAT_UNIFORM0(4, 2, prefix, mat_p) \
	MAT_UNIFORM0(4, 3, prefix, mat_p) 

namespace glsl_lib
{
	const std::string shader::mvp_name = std::string("MVP");



	namespace _detail
	{
		void uniform(GLint loc, float v)
		{
			glUniform1f(loc, v);
		}
		void uniform(GLint loc, double v)
		{
			glUniform1d(loc, v);
		}
		void uniform(GLint loc, unsigned int v)
		{
			glUniform1ui(loc, v);
		}
		void uniform(GLint loc, int v)
		{
			glUniform1i(loc, v);
		}
		void uniform(GLint loc, bool v)
		{
			glUniform1i(loc, static_cast<int>(v));
		}

		UNIFORM(f, );
		UNIFORM(d, d);
		UNIFORM(ui, u);
		UNIFORM(i, i);

		MAT_UNIFORM(f, );
		MAT_UNIFORM(d, d);
	}

	void texture::bind()
	{
		GLenum tgt = 0;
		switch (target)
		{
		case TEXTURE_1D:
			tgt = GL_TEXTURE_1D;
			break; 
		case TEXTURE_2D:
			tgt = GL_TEXTURE_2D;
			break;
		case TEXTURE_3D:
			tgt = GL_TEXTURE_3D;
			break;
		case TEXTURE_1D_ARRAY:
			tgt = GL_TEXTURE_1D_ARRAY;
			break;
		case TEXTURE_2D_ARRAY:
			tgt = GL_TEXTURE_2D_ARRAY;
			break;
		case TEXTURE_RECTANGLE:
			tgt = GL_TEXTURE_RECTANGLE;
			break;
		case TEXTURE_CUBE_MAP:
			tgt = GL_TEXTURE_CUBE_MAP;
			break;
		case TEXTURE_CUBE_MAP_ARRAY:
			tgt = GL_TEXTURE_CUBE_MAP_ARRAY;
			break;
		case TEXTURE_BUFFER:
			tgt = GL_TEXTURE_BUFFER;
			break;
		case TEXTURE_2D_MULTISAMPLE:
			tgt = GL_TEXTURE_2D_MULTISAMPLE;
			break;
		case TEXTURE_2D_MULTISAMPLE_ARRAY:
			tgt = GL_TEXTURE_2D_MULTISAMPLE_ARRAY;
			break;
		}

		glBindTexture(tgt, id);
	}

	void a()
	{
		std::string b;


	}
}
