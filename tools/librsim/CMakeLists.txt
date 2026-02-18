cmake_minimum_required(VERSION 3.10)
project(rsim C)

set(CMAKE_C_STANDARD 11)
set(CMAKE_POSITION_INDEPENDENT_CODE ON)

add_library(rsim SHARED src/rsim_x11.c)
target_link_libraries(rsim X11)

# Produces librsim.so (Linux convention)
set_target_properties(rsim PROPERTIES OUTPUT_NAME "rsim")
