include_guard()
include(GNUInstallDirs)

function(tdotnet_dotnet_path path)
    if(DEFINED DOTNET_EXECUTABLE_PATH)
        set(${path} ${DOTNET_EXECUTABLE_PATH} PARENT_SCOPE)
        return()
    endif()

    find_program(DOTNET_FOUND dotnet)

    if(NOT DOTNET_FOUND)
        message(FATAL_ERROR "Could not find .NET.")
    else()
        message(STATUS "dotnet executable found at ${DOTNET_FOUND}")
        set(DOTNET_EXECUTABLE_PATH ${DOTNET_FOUND} CACHE STRING "Path to the dotnet executable")
        set(${path} ${DOTNET_EXECUTABLE_PATH} PARENT_SCOPE)
    endif()
endfunction()

function(tdotnet_generator_path path)
    if(DEFINED TDOTNETBRIDGE_GENERATOR_PATH)
        set(${path} ${TDOTNETBRIDGE_GENERATOR_PATH} PARENT_SCOPE)
        return()
    endif()

    find_program(TDOTNETBRIDGE_GENERATOR_FOUND tdotnetbridge-generator)

    if(NOT TDOTNETBRIDGE_GENERATOR_FOUND)
        message(FATAL_ERROR "Could not find tdotnetbridge-generator")
    else()
        message(STATUS "tdotnetbridge-generator executable found at ${TDOTNETBRIDGE_GENERATOR_FOUND}")
        set(TDOTNETBRIDGE_GENERATOR_PATH ${TDOTNETBRIDGE_GENERATOR_FOUND} CACHE STRING "Path to the tdotnetbridge-generator executable")
        set(${path} ${TDOTNETBRIDGE_GENERATOR_PATH} PARENT_SCOPE)
    endif()
endfunction()

function(tdotnet_add_dotnet_project name)
    set(options NO_GENERATE NO_INSTALL)  # Boolean arguments
    set(oneValueArgs PROJECT SELF_CONTAINED)   # Single value arguments
    set(multiValueArgs ) # Multi value arguments

    cmake_parse_arguments(ADD_DOTNET_PROJECT "${options}" "${oneValueArgs}" "${multiValueArgs}" ${ARGN})

    tdotnet_dotnet_path(DOTNET_PATH)

    if(NOT TARGET restore)
        add_custom_target(restore)
    endif()

    add_custom_target(
        ${name}_dotnet_restore ALL
        COMMAND ${DOTNET_PATH} restore ${CMAKE_CURRENT_SOURCE_DIR}/${ADD_DOTNET_PROJECT_PROJECT}
        COMMENT "Restoring dependencies for MSBuild project ${ADD_DOTNET_PROJECT_PROJECT}..."
    )

    set(output_binary_directory ${name})
    set(publish_directory ${name})
    set(publish_args -c Release)
    set(runtimes)

    if(ADD_DOTNET_PROJECT_SELF_CONTAINED)
        # Make the result self contained
        list(APPEND publish_args --self-contained)
    endif()

    if(APPLE)
        # Create a bundle
        set(output_binary_directory ${name}.framework/Contents/MacOS)
        set(publish_directory ${name}.framework)

        # Set the runtimes
        list(APPEND runtimes osx-x64 osx-arm64)
    endif()


    add_custom_target(${name}_dotnet_build ALL)
    foreach(runtime IN LISTS runtimes)
        add_custom_target(
            ${name}_dotnet_build_${runtime}
            COMMAND ${DOTNET_PATH} publish ${publish_args} -r ${runtime} ${CMAKE_CURRENT_SOURCE_DIR}/${ADD_DOTNET_PROJECT_PROJECT} --output ${CMAKE_CURRENT_BINARY_DIR}/tdotnet/${output_binary_directory}/${runtime} --no-restore
            COMMENT "Building MSBuild project ${ADD_DOTNET_PROJECT_PROJECT}..."
        )
        add_dependencies(${name}_dotnet_build ${name}_dotnet_build_${runtime})
        add_dependencies(${name}_dotnet_build_${runtime} ${name}_dotnet_restore)
    endforeach()

    if(NOT ADD_DOTNET_PROJECT_NO_INSTALL)
        install(DIRECTORY ${CMAKE_CURRENT_BINARY_DIR}/tdotnet/${publish_directory}
            DESTINATION ${CMAKE_INSTALL_LIBDIR}/tdotnetbridge
            USE_SOURCE_PERMISSIONS
        )
    endif()

    add_library(${name} INTERFACE)
    add_dependencies(${name} ${name}_dotnet_build)
    add_dependencies(${name}_dotnet_build ${name}_dotnet_restore)
    add_dependencies(restore ${name}_dotnet_restore)

    if(NOT ADD_DOTNET_PROJECT_NO_GENERATE)
        tdotnet_generator_path(GENERATOR_PATH)

        # Find out what files need to be added to the target
        message(STATUS "Discovering generated files for MSBuild project ${ADD_DOTNET_PROJECT_PROJECT}...")
        execute_process(
            COMMAND ${GENERATOR_PATH} -n ${CMAKE_CURRENT_SOURCE_DIR}/${ADD_DOTNET_PROJECT_PROJECT} ${CMAKE_CURRENT_BINARY_DIR}/tdotnet-include/${name}
            OUTPUT_VARIABLE GENERATOR_DRY_RUN_OUTPUT
        )

        # Replace newline with semicolon
        string(REPLACE "\n" ";" GENERATOR_DRY_RUN_OUTPUT_LIST "${GENERATOR_DRY_RUN_OUTPUT}")

        set(GENERATOR_OUTPUT_FILES "")
        foreach(item ${GENERATOR_DRY_RUN_OUTPUT_LIST})
            list(APPEND GENERATOR_OUTPUT_FILES "${CMAKE_CURRENT_BINARY_DIR}/tdotnet-include/${name}/${item}")
        endforeach()

        message(TRACE "${GENERATOR_OUTPUT_FILES}")

        add_custom_command(
#            ${name}_dotnet_generate ALL
            OUTPUT ${GENERATOR_OUTPUT_FILES}
            COMMAND ${GENERATOR_PATH} ${CMAKE_CURRENT_SOURCE_DIR}/${ADD_DOTNET_PROJECT_PROJECT} ${CMAKE_CURRENT_BINARY_DIR}/tdotnet-include/${name}
            COMMENT "Generating CXX glue for MSBuild project ${ADD_DOTNET_PROJECT_PROJECT}..."
        )
        add_library(${name}_dotnet_generate STATIC ${GENERATOR_OUTPUT_FILES})
        set_target_properties(${name}_dotnet_generate PROPERTIES
            LINKER_LANGUAGE CXX
            CXX_STANDARD 20
            AUTOMOC ON
            POSITION_INDEPENDENT_CODE ON
        )
        target_link_libraries(${name}_dotnet_generate PUBLIC Qt::Core tdotnetbridge)
        target_include_directories(${name} INTERFACE ${CMAKE_CURRENT_BINARY_DIR}/tdotnet-include)
        add_dependencies(${name} ${name}_dotnet_generate)
        target_link_libraries(${name} INTERFACE ${name}_dotnet_generate)
    endif()
endfunction()
