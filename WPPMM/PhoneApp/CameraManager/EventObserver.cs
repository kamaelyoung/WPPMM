using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using System.Windows;
using WPPMM.RemoteApi;

namespace WPPMM.CameraManager
{
    public class EventObserver
    {
        private readonly CameraServiceClient10 client;

        private int failure_count = 0;

        private const int RETRY_LIMIT = 3;

        private const int RETRY_INTERVAL_SEC = 3;

        private CameraStatus status;

        private Action<EventMember> OnDetectDifference = null;

        private Action OnStop = null;

        private BackgroundWorker worker = new BackgroundWorker()
        {
            WorkerReportsProgress = false,
            WorkerSupportsCancellation = true
        };

        public EventObserver(CameraServiceClient10 client)
        {
            this.client = client;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="status">Status object to update</param>
        /// <param name="OnDetectDifference">Called when the parameter has been changed</param>
        /// <param name="OnStop">Called when event observation is finished with error</param>
        ///
        public void Start(CameraStatus status, Action<EventMember> OnDetectDifference, Action OnStop)
        {
            Debug.WriteLine("EventObserver.Start");
            if (status == null | OnDetectDifference == null || OnStop == null)
            {
                throw new ArgumentNullException();
            }
            status.InitEventParams();
            this.status = status;
            this.OnDetectDifference = OnDetectDifference;
            this.OnStop = OnStop;
            failure_count = RETRY_LIMIT;
            worker.DoWork += AnalyzeEventData;
            client.GetEvent(false, OnError, OnSuccess);
        }

        /// <summary>
        /// Force finish event observation. Any callbacks will not be invoked after this.
        /// </summary>
        public void Stop()
        {
            Debug.WriteLine("EventObserver.Stop");
            Deactivate();
        }

        private void OnError(int code)
        {
            switch (code)
            {
                case StatusCode.NotAcceptable:
                case StatusCode.CameraNotReady:
                case StatusCode.IllegalState:
                case StatusCode.ServiceUnavailable:
                case StatusCode.Timeout:
                case StatusCode.Any:
                    if (++failure_count < RETRY_LIMIT)
                    {
                        Debug.WriteLine("GetEvent failed: retry " + failure_count);
                        Timer timer = new Timer((state) =>
                        {
                            Call();
                        }, null, TimeSpan.FromSeconds(RETRY_INTERVAL_SEC), new TimeSpan(-1));
                        return;
                    }
                    break;
                case StatusCode.DuplicatePolling:
                    Debug.WriteLine("GetEvent failed duplicate polling");
                    return;
                default:
                    Debug.WriteLine("GetEvent failed with code: " + code);
                    break;
            }

            if (OnStop != null)
            {
                OnStop.Invoke();
            }
            Debug.WriteLine("GetEvent Error limit: deactivate now");
            Deactivate();
        }

        private void OnSuccess(Event data)
        {
            failure_count = 0;
            try
            {
                worker.RunWorkerAsync(data);
            }
            catch (InvalidOperationException)
            {
                return;
            }
        }

        private void AnalyzeEventData(object sender, DoWorkEventArgs e)
        {
            if (status == null)
            {
                return;
            }
            CameraStatus target = status;

            var data = e.Argument as Event;
            if (StatusComparator.IsAvailableApisModified(target, data.AvailableApis))
                NotifyChangeDetected(EventMember.AvailableApis);

            if (StatusComparator.IsCameraStatusModified(target, data.CameraStatus))
                NotifyChangeDetected(EventMember.CameraStatus);

            if (StatusComparator.IsLiveviewAvailableModified(target, data.LiveviewAvailable))
                NotifyChangeDetected(EventMember.LiveviewAvailable);

            if (StatusComparator.IsPostviewSizeInfoModified(target, data.PostviewSizeInfo))
                NotifyChangeDetected(EventMember.PostviewSizeInfo);

            if (StatusComparator.IsSelftimerInfoModified(target, data.SelfTimerInfo))
                NotifyChangeDetected(EventMember.SelfTimerInfo);

            if (StatusComparator.IsShootModeInfoModified(target, data.ShootModeInfo))
                NotifyChangeDetected(EventMember.ShootModeInfo);

            if (StatusComparator.IsZoomInfoModified(target, data.ZoomInfo))
                NotifyChangeDetected(EventMember.ZoomInfo);

            if (StatusComparator.IsExposureModeInfoModified(target, data.ExposureMode))
                NotifyChangeDetected(EventMember.ExposureMode);

            if (StatusComparator.IsFNumberModified(target, data.FNumber))
                NotifyChangeDetected(EventMember.FNumber);

            if (StatusComparator.IsShutterSpeedModified(target, data.ShutterSpeed))
                NotifyChangeDetected(EventMember.ShutterSpeed);

            if (StatusComparator.IsISOModified(target, data.ISOSpeedRate))
                NotifyChangeDetected(EventMember.ISOSpeedRate);

            if (StatusComparator.IsEvInfoModified(target, data.EvInfo))
                NotifyChangeDetected(EventMember.EVInfo);

            if (StatusComparator.IsProgramShiftModified(target, data.ProgramShiftActivated))
                NotifyChangeDetected(EventMember.ProgramShift);

            Call();
        }

        private void Call()
        {
            if (status != null)
            {
                client.GetEvent(true, OnError, OnSuccess);
            }
        }

        private void NotifyChangeDetected(EventMember target)
        {
            //Debug.WriteLine("NotifyChangeDetected: " + target);
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                if (OnDetectDifference != null)
                { OnDetectDifference.Invoke(target); }
            });
        }

        private void Deactivate()
        {
            Debug.WriteLine("EventObserver deactivated");
            status = null;
            OnStop = null;
            OnDetectDifference = null;
            worker.DoWork -= AnalyzeEventData;
        }
    }

    public enum EventMember
    {
        AvailableApis,
        CameraStatus,
        ZoomInfo,
        LiveviewAvailable,
        PostviewSizeInfo,
        SelfTimerInfo,
        ShootModeInfo,
        ExposureMode,
        ShutterSpeed,
        FNumber,
        ISOSpeedRate,
        EVInfo,
        ProgramShift
    }
}
