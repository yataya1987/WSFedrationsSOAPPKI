/* MIT License
 *
 * Copyright (c) 2018 LogiQ System
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 */

using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using CronQuery.Cron;

namespace CronQuery.Mvc.Jobs
{
    public sealed class JobInterval : IDisposable
    {
        private CronExpression _cron;
        private TimeZoneInfo _timezone;
        private Func<Task> _work;
        private IDisposable _subscription;

        public JobInterval(CronExpression cron, TimeZoneInfo timezone, Func<Task> work)
        {
            _cron = cron ?? throw new ArgumentNullException(nameof(cron));
            _timezone = timezone ?? throw new ArgumentNullException(nameof(timezone));
            _work = work ?? throw new ArgumentNullException(nameof(work));
        }

        public void Dispose()
        {
            if (_subscription != null)
            {
                _subscription.Dispose();
            }
        }

        public void Run()
        {
            var now = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, _timezone);
            var nextTime = _cron.Next(now);

            if (nextTime == DateTime.MinValue)
            {
                return;
            }

            var interval = nextTime - now;

            _subscription = Observable.Timer(interval)
                .Select(tick => Observable.FromAsync(_work))
                .Concat()
                .Subscribe(
                    onNext: tick => { /* noop */ },
                    onCompleted: Run
                );
        }
    }
}
