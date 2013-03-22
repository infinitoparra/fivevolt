
using System;
using ZigBeeCoordinator.Lib.Models.Db;

namespace ZigBeeCoordinator.Lib.Models
{
    public class ZigBeeCoordinatorContext : IDisposable
    {
        private readonly ZigBeeCoordinatorEntities _dbCtx;

        public ZigBeeCoordinatorEntities DbCtx { get { return _dbCtx; } }
        public static string ConnectionString { get; set; }

        public ZigBeeCoordinatorContext()
        {
            if (string.IsNullOrEmpty(ConnectionString))
            {
                _dbCtx = new ZigBeeCoordinatorEntities();
            }
            else
            {
                _dbCtx = new ZigBeeCoordinatorEntities(ConnectionString);
            }
        }

        public void Dispose()
        {
            _dbCtx.Dispose();
        }
    }
}
