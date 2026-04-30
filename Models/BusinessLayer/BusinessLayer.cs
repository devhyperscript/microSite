using firstproject.Models.DatabaseLayer;

namespace firstproject.Models.BusinessLayer
{
    public partial interface IBusinessLayer
    {

    }

    public partial class BusinessLayer : IBusinessLayer
    {
        private readonly IDatabaseLayer _databaseLayer;


        public BusinessLayer(
            IDatabaseLayer dataBaseLayer
            )
        {
            this._databaseLayer = dataBaseLayer;
        }


    }
}
