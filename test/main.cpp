#include <QCoreApplication>

#include "testobjectclass.h"
#include <QTextStream>
#include <include/qdotnetevent.h>

int main(int argc, char** argv) {
    QCoreApplication a(argc, argv);
    QDotNetHost dotnetHost;
    dotnetHost.load();

    if (!dotnetHost.isLoaded()) {
        QTextStream(stderr) << "Not loaded\n";
    } else {
        QTextStream(stderr) << ".NET ready\n";
    }

    QDotNetFunction<void> helloWorldFunction;
    dotnetHost.resolveFunction(helloWorldFunction, "/home/victor/RiderProjects/ClassLibrary2/ClassLibrary2/bin/Debug/net7.0/ClassLibrary2.dll", "ClassLibrary2.Class1, ClassLibrary2", "HelloWorld", "ClassLibrary2.Class1+HelloWorldDelegate, ClassLibrary2");
    helloWorldFunction();

    QDotNetAdapter::init("/home/victor/Documents/Apps/tdotnetbridge/bridge/tdotnetbridge/tdotnetbridge.Adapter/bin/Debug/net7.0/tdotnetbridge.Adapter.dll", "tdotnetbridge.Adapter", "Qt.DotNet.Adapter");
    QDotNetAdapter::instance().loadAssembly("/home/victor/RiderProjects/ClassLibrary2/ClassLibrary2/bin/Debug/net7.0/ClassLibrary2.dll");

    TestObjectClass testDotNetClass;
    testDotNetClass.writeMessage("Message from Qt");

    testDotNetClass.writeMessage(QStringLiteral("1 + 2 yields %1").arg(testDotNetClass.add(1, 2)));

    TestObjectClass testDotNetClassWithPrefix(QStringLiteral("prefix: "));
    testDotNetClassWithPrefix.writeMessage("prefixed message");

    testDotNetClassWithPrefix.setCustomProperty("Custom Property Here");
    testDotNetClassWithPrefix.writeMessage(testDotNetClassWithPrefix.customProperty());
    testDotNetClassWithPrefix.setSetOnlyProperty("Set Only");

    return a.exec();
}
