project(lib VERSION 1.0.0 LANGUAGES CXX)

find_package(Qt${QT_VERSION_MAJOR} REQUIRED Core)
include(CMakePackageConfigHelpers)

file(GLOB EXTRA_FUNCTIONS "${CMAKE_CURRENT_LIST_DIR}/cmake-scripts/*.cmake")
foreach(f ${EXTRA_FUNCTIONS})
    include(${f})
endforeach()

set(SOURCES

    )

set(HEADERS
        include/qdotnetadapter.h
        include/qdotnetarray.h
        include/qdotnetcallback.h
        include/qdotnetevent.h
        include/qdotnetexception.h
        include/qdotnetfunction.h
        include/qdotnethost.h
        include/qdotnethostfxr.h
        include/qdotnetinterface.h
        include/qdotnetmarshal.h
        include/qdotnetobject.h
        include/qdotnetparameter.h
        include/qdotnetref.h
        include/qdotnetsafemethod.h
        include/qdotnettype.h
    )

add_library(tdotnetbridge SHARED ${SOURCES} ${HEADERS}
    cmake-scripts/cmake-dotnet-add.cmake)
set_target_properties(tdotnetbridge PROPERTIES
        CXX_STANDARD 20
        LINKER_LANGUAGE CXX
        FRAMEWORK TRUE
        MACOSX_FRAMEWORK_IDENTIFIER com.vicr123.tdotnetbridge
        VERSION 1.0.0
        PUBLIC_HEADER "${HEADERS}")

target_link_libraries(tdotnetbridge Qt::Core)
target_compile_definitions(tdotnetbridge PRIVATE TDOTNETBRIDGE_LIBRARY)

IF(${CMAKE_SYSTEM_NAME} MATCHES "Darwin")
    set(CMAKECONFIG_INSTALL_DIR ${CMAKE_CURRENT_BINARY_DIR}/tdotnetbridge.framework/Resources/CMake)
    set(HEADER_INSTALL_DIR ${CMAKE_INSTALL_PREFIX})
    set(INCLUDE_INSTALL_DIR ${CMAKE_INSTALL_LIBDIR}/tdotnetbridge.framework/Headers)
    set(LIBRARY_INSTALL_DIR ../)
ELSE()
    set(CMAKECONFIG_INSTALL_DIR ${CMAKE_INSTALL_LIBDIR}/cmake/tdotnetbridge)
    set(HEADER_INSTALL_DIR ${CMAKE_INSTALL_INCLUDEDIR}/tdotnetbridge)
    set(INCLUDE_INSTALL_DIR ${HEADER_INSTALL_DIR})
    set(LIBRARY_INSTALL_DIR ${CMAKE_INSTALL_LIBDIR})
ENDIF()

tdotnet_add_dotnet_project(tdotnetbridge-adapter NO_GENERATE
    PROJECT tdotnetbridge/tdotnetbridge.Adapter/tdotnetbridge.Adapter.csproj)
tdotnet_add_dotnet_project(tdotnetbridge-generator NO_GENERATE
    PROJECT tdotnetbridge/tdotnetbridge.Generator/tdotnetbridge.Generator.csproj)

configure_package_config_file(tdotnetbridgeConfig.cmake.in ${CMAKE_CURRENT_BINARY_DIR}/tdotnetbridgeConfig.cmake
        INSTALL_DESTINATION ${CMAKECONFIG_INSTALL_DIR}
        PATH_VARS HEADER_INSTALL_DIR LIBRARY_INSTALL_DIR)

install(FILES ${CMAKE_CURRENT_BINARY_DIR}/tdotnetbridgeConfig.cmake
        DESTINATION ${CMAKECONFIG_INSTALL_DIR})
install(EXPORT tdotnetbridgeTargets
        FILE tdotnetbridgeTargets.cmake
        DESTINATION ${CMAKECONFIG_INSTALL_DIR})
install(DIRECTORY cmake-scripts
        DESTINATION ${CMAKECONFIG_INSTALL_DIR})

install(TARGETS tdotnetbridge
        EXPORT tdotnetbridgeTargets
        PUBLIC_HEADER DESTINATION ${HEADER_INSTALL_DIR}
        INCLUDES DESTINATION ${INCLUDE_INSTALL_DIR}
        LIBRARY DESTINATION ${CMAKE_INSTALL_LIBDIR}
        FRAMEWORK DESTINATION ${CMAKE_INSTALL_LIBDIR})

install(CODE "
    message(\"-- Linking tdotnetbridge-generator\")
    file(CREATE_LINK \"${CMAKE_INSTALL_FULL_LIBDIR}/tdotnetbridge/tdotnetbridge-generator/tdotnetbridge.Generator\" \"\$ENV{DESTDIR}${CMAKE_INSTALL_FULL_BINDIR}/tdotnetbridge-generator\" SYMBOLIC)
")
