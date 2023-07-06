using LiqunManagement.Models;
using NLog;
using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure.Interception;
using System.Linq;
using System.Web;

namespace LiqunManagement.Services
{
    public class BaseService
    {
        //protected BoardModel Boarddb = new BoardModel();
        public MembersModel Memberdb = new MembersModel();

        public LiqunModels Liqundb = new LiqunModels();

        public Logger logger;

        public BaseService()
        {
            logger = LogManager.GetCurrentClassLogger();
        }

        //public void Dispose()
        //{
        //    Boarddb.Dispose();
        //}
    }
}