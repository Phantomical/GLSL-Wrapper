# GLSL-Wrapper
A compiler and runtime for glsl shader abstraction.

## Compiler Arguments
 - out=[file] : This option sets the output file. (The default is 'shader_file.h' in the current directory)
 - name=[identifier] : This option sets the name of the output shader class. (The default is '__Shader[random number]')
 - [file] : Lists a file to be compiled as one of the shader stages. (The stage is inferred from the file extension, if the stage cannot be inferred then the argument will be ignored)
 - vert=[file] : Compiles the file as a vertex shader stage.
 - frag=[file] : Compiles the file as a fragment shader stage.
 - geom=[file] : Compiles the file as a geometry shader stage.
 - tessEval=[file] : Compiles the file as a tesselation evaluation shader stage.
 - tessControl=[file] : Compiles the file as a tesselation control shader stage.
 - compute=[file] : Compiles the file as a compute shader. (This argument cannot be specified with any other stage types)
