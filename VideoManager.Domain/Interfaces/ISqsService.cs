using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VideoManager.Domain.Entities;

namespace VideoManager.Domain.Interfaces
{
    public interface ISqsService
    {
        Task SendAsync(Video video);
    }
}
