include_guard()
include(GNUInstallDirs)

function(tdotnet_dotnet_path path)
    if(NOT DEFINED DOTNET_EXECUTABLE_PATH)
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

function(tdotnet_add_dotnet_project name)
    set(options NO_GENERATE NO_INSTALL)  # Boolean arguments
    set(oneValueArgs PROJECT)   # Single value arguments
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

    add_custom_target(
        ${name}_dotnet_build ALL
        COMMAND ${DOTNET_PATH} publish -c Release ${CMAKE_CURRENT_SOURCE_DIR}/${ADD_DOTNET_PROJECT_PROJECT} --output ${CMAKE_CURRENT_BINARY_DIR}/tdotnet/${name} --no-restore
        COMMENT "Building MSBuild project ${ADD_DOTNET_PROJECT_PROJECT}..."
    )

    if(NOT NO_INSTALL)
        install(DIRECTORY ${CMAKE_CURRENT_BINARY_DIR}/tdotnet/${name}
            DESTINATION ${CMAKE_INSTALL_LIBDIR}/tdotnetbridge
            USE_SOURCE_PERMISSIONS
        )
    endif()

    add_custom_target(${name})
    add_dependencies(${name} ${name}_dotnet_build)
    add_dependencies(${name}_dotnet_build ${name}_dotnet_restore)
    add_dependencies(restore ${name}_dotnet_restore)
endfunction()
