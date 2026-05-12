cmake_minimum_required(VERSION 3.20)

project(gojoSIMD
    VERSION 0.1.0
    DESCRIPTION "Native C++ SIMD-shaped Gojo kernel experiment"
    LANGUAGES CXX)

add_library(gojoSIMD
    src/gojo.cpp)

target_include_directories(gojoSIMD
    PUBLIC
        $<BUILD_INTERFACE:${CMAKE_CURRENT_SOURCE_DIR}/include>
        $<INSTALL_INTERFACE:include>)

target_compile_features(gojoSIMD PUBLIC cxx_std_20)

if (MSVC)
    target_compile_options(gojoSIMD PRIVATE /W4 /permissive-)
else()
    target_compile_options(gojoSIMD PRIVATE -Wall -Wextra -Wpedantic)
endif()

add_executable(gojoSIMD_tests
    tests/gojoSIMD_tests.cpp)

target_link_libraries(gojoSIMD_tests PRIVATE gojoSIMD)

enable_testing()
add_test(NAME gojoSIMD_tests COMMAND gojoSIMD_tests)
