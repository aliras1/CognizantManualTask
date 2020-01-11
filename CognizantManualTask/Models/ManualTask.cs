using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CognizantManualTask.Models
{
    public class ManualTaskView
    {
        public enum State
        {
            Idle,
            Pass,
            Fail
        }

        public long Id { get; set; }
        public State CurrentState { get; set; }
    }
}
