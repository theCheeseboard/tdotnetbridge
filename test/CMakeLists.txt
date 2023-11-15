project(tdotnetbridge-test VERSION 1.0.0 LANGUAGES CXX)

find_package(Qt${QT_VERSION_MAJOR} REQUIRED COMPONENTS Widgets DBus Sql PrintSupport)
find_package(QCoro${QT_VERSION_MAJOR} REQUIRED COMPONENTS Core)

set(SOURCES
        main.cpp
    )

set(HEADERS
        testobjectclass.h
    )

# Find Apple lproj files
set(LPROJ_SOURCES)
file(GLOB LPROJ_DIRS ${CMAKE_CURRENT_SOURCE_DIR}/translations/apple-lproj/*.lproj)
FOREACH(DIR ${LPROJ_DIRS})
    cmake_path(GET DIR FILENAME DIR_NAME)
    file(GLOB LPROJ_FILES ${DIR}/*.strings)
    FOREACH(FILE ${LPROJ_FILES})
        list(APPEND LPROJ_SOURCES ${FILE})
        set_source_files_properties(${FILE} PROPERTIES
                MACOSX_PACKAGE_LOCATION Resources/${DIR_NAME})
    ENDFOREACH()
ENDFOREACH()
set(SOURCES ${SOURCES} ${LPROJ_SOURCES})

add_executable(tdotnetbridge-test ${SOURCES} ${HEADERS})

set_target_properties(tdotnetbridge-test PROPERTIES
    AUTOMOC TRUE
    CXX_STANDARD 20
        MACOSX_BUNDLE TRUE
#        MACOSX_BUNDLE_INFO_PLIST ${CMAKE_CURRENT_SOURCE_DIR}/Info.plist.in
        MACOSX_BUNDLE_GUI_IDENTIFIER com.vicr123.tdotnetbridge-test
        WIN32_EXECUTABLE TRUE
        CNTP_DATA_SUBDIR tdotnetbridge-test
        TRANSLATION_SOURCES "${SOURCES};${HEADERS}"
    )

target_link_libraries(tdotnetbridge-test tdotnetbridge QCoro::Core)
target_include_directories(tdotnetbridge-test PUBLIC ../bridge/)

install(TARGETS tdotnetbridge-test
        RUNTIME DESTINATION ${CMAKE_INSTALL_BINDIR}
        BUNDLE DESTINATION /Applications)
