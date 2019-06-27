using DevExpress.EntityFramework.SecurityDataStore;
using DevExpress.EntityFramework.SecurityDataStore.Security.BaseSecurityEntity;
using System.Linq;

namespace EFCoreSecurityConsoleDemo.DataModel {
    public class Contact : BaseSecurityEntity {
        private string name;
        private string address;

        public int Id { get; set; }
        public string Name {
            get {
                return this.GetValue(name, "Name");
            }
            set {
                name = value;
            }
        }
        public string Address {
            get {
                return this.GetValue(address, "Address");
            }
            set {
                address = value;
            }
        }
    }
}
