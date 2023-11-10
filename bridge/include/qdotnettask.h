#ifndef QDOTNETTASK_H
#define QDOTNETTASK_H

#include <QTimer>
#include <coroutine.h>
#include <functional>
#include <include/qdotnetobject.h>

template<typename T> class QDotNetTask : public QDotNetObject {
    public:
        Q_DOTNET_OBJECT_INLINE(QDotNetTask, "System.Threading.Tasks.Task");

        T result() {
            return method("get_Result", _fn_Result).invoke(*this);
        }

        bool isCompleted() {
            return method("get_IsCompleted", _fn_is_completed).invoke(*this);
        }

        bool isFaulted() {
            return method("get_IsFaulted", _fn_is_faulted).invoke(*this);
        }

        bool isCancelled() {
            return method("get_IsCancelled", _fn_is_cancelled).invoke(*this);
        }

        // coroutines

        struct promise_type {
                QDotNetTask<T> get_return_object() { return {}; }
                std::suspend_never initial_suspend() { return {}; }
                std::suspend_never final_suspend() { return {}; }
                void return_void() {}
                void unhandled_exception() {}
        };

        bool await_ready() {
            return this->isCompleted() || this->isFaulted();
        }

        T await_resume() {
            return this->result();
        }

        template<typename HandleType>
        void await_suspend(std::coroutine_handle<HandleType> coroutineHandle) {
            QTimer* timer = new QTimer();
            timer->setInterval(0);
            QObject::connect(timer, &QTimer::timeout, [this, coroutineHandle, timer] {
                if (this->await_ready()) {
                    coroutineHandle.resume();
                    timer->deleteLater();
                }
            });
            timer->start();
        }

    private:
        QDotNetFunction<T> _fn_Result;
        QDotNetFunction<bool> _fn_is_completed;
        QDotNetFunction<bool> _fn_is_faulted;
        QDotNetFunction<bool> _fn_is_cancelled;
};

#endif // QDOTNETTASK_H
