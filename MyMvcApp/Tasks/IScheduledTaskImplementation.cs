﻿using System.Threading.Tasks;

namespace MyMvcApp.Tasks
{
    public interface IScheduledTaskImplementation
    {
        Task Execute(IScheduledTaskHost taskHost);
    }
}
