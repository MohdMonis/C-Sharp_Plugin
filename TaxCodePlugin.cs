using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace TaxCodePlugin
{
    public class TaxCodePlugin : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            try
            {
                ITracingService tracer = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
                IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
                IOrganizationServiceFactory serviceFactory = ((IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory)));
                IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);

                tracer.Trace("Inside TaxCodePlugin");
                if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
                {
                    Entity targetEn = (Entity)context.InputParameters["Target"];
                    tracer.Trace("targetEn.Id=" + targetEn.Id);
                    Guid existingProductGuid = Guid.Empty;
                    Entity existingProduct = null;
                    Guid taxCodeGuid = Guid.Empty;
                    targetEn = service.Retrieve(targetEn.LogicalName, targetEn.Id, new ColumnSet("productid", "new_taxcode"));
                    tracer.Trace("New TargetEn.Id=" + targetEn.Id);
                    if (targetEn.Contains("productid") && !targetEn.Contains("new_taxcode"))
                    {
                        tracer.Trace("Condition met");
                        existingProductGuid = ((EntityReference)targetEn["productid"]).Id;
                        existingProduct = service.Retrieve("product", existingProductGuid, new ColumnSet("new_taxcode"));
                        tracer.Trace("existingProductGuid : " + existingProductGuid);
                        if (existingProduct.Contains("new_taxcode"))
                        {
                            tracer.Trace(" : Product Contains TaxCode : ");
                            taxCodeGuid = ((EntityReference)existingProduct["new_taxcode"]).Id;
                            tracer.Trace("Get TaxCodeId: " + taxCodeGuid);
                            targetEn["new_taxcode"] = new EntityReference("new_taxcode", taxCodeGuid);
                            service.Update(targetEn);
                            tracer.Trace("task of updating field of order product on adding existing product to it was sucessfully completed!..");
                        }
                    }
                //throw new Exception("Testing TaxCodePlugin exception");
                }
            }
            catch (Exception ex)
            {
                throw new InvalidPluginExecutionException("TaxCodePlugin|Eror : " + ex.Message);
            }
        }
    }
}
