using System;
using System.Collections.Generic;
using System.IO;
using Commons.Collections;
using NVelocity;
using NVelocity.App;
using NVelocity.Runtime;

namespace com.Sconit.Utility
{
    public class NVelocityTemplateRepository
    {
        public enum TemplateEnum
        {
            /// <summary>
            /// //导入SAP物料失败
            /// </summary>
            ImportSapItemFail = 10101,

            /// <summary>
            ///   //导入SAP供应商失败
            /// </summary>
            ImportSapSupplierFail = 10201,

            /// <summary>
            ///  //导入SAP整车生产单失败
            /// </summary>
            ImportVanPordOrderFail = 10301,

            /// <summary>
            /// //导入SAP采购单失败
            /// </summary>
            ImportProcOrderFail = 10501,

            /// <summary>
            /// //导入SAP配额失败
            /// </summary>
            ImportSapQuotaFail = 10601,

            /// <summary>
            ///  //导出生产单报工失败
            /// </summary>
            ReportProdOrderOpFail = 10801,

            /// <summary>
            ///  //自制件物料反冲失败
            /// </summary>
            BackflushProductionOrder = 10802,

            /// <summary>
            /// //创建交货单失败 
            /// </summary>
            ImportSapDO_ImportFail = 10901,

            /// <summary>
            /// //调用SAP交货单过账失败 
            /// </summary>
            ImportSapDO_PostFail = 10902,

            /// <summary>
            /// //生成库存报表失败 
            /// </summary>
            GenerateInvRepFail = 11301,
           
            /// <summary>
            /// 反写SAP计划协议失败
            /// </summary>
            GenerateSAPScheduleLine = 11302,

            /// <summary>
            /// //导入移动类型连接Web服务失败
            /// </summary>
            ImportSapMoveType_WebServiceNotAccess = 10701,
            /// <summary>
            /// //移动类型导入到中间表失败
            /// </summary>
            ImportSapMoveType_CreateInvTransFail = 10702,
            /// <summary>
            /// //导入移动类型读取LES数据失败
            /// </summary>
            ImportSapMoveType_LesError = 10703,
            /// <summary>
            /// //更新移动类型中间表失败
            /// </summary>
            ImportSapMoveType_UpdateInvTransFail = 10704,
            /// <summary>
            /// //导入移动类型记录表之间关系失败
            /// </summary>
            ImportSapMoveType_CreateInvLocFail = 10705,
            /// <summary>
            /// //导入移动类型记录SAP返回结果失败
            /// </summary>
            ImportSapMoveType_CreateTransCallBackFail = 10706,
            /// <summary>
            /// //导入盘点移动类型中失败
            /// </summary>
            ImportSapMoveType_StockTake_LesServiceError = 10707,

            /// <summary>
            ///   //整车生产单暂停
            /// </summary>
            PauseVanProdOrder = 12101,

            /// <summary>
            ///   //整车生产单恢复
            /// </summary>
            RestartVanProdOrder = 12102,

            /// <summary>
            /// Kit单修改
            /// </summary>
            SeqOrderChange=12103,

            /// <summary>
            /// 看板订单创建失败
            /// </summary>
            GenAnDonOrderFail = 12104,

            /// <summary>
            /// 精益引擎运行异常
            /// </summary>
            LeanEngineRunException = 12105,
        }

        public IDictionary<int, string> templateNameDictionary { get; set; }
        private VelocityEngine engine;

        public NVelocityTemplateRepository(string templateDirectory)
        {
            engine = new VelocityEngine();
            ExtendedProperties props = new ExtendedProperties();
            props.AddProperty(RuntimeConstants.FILE_RESOURCE_LOADER_PATH, templateDirectory);

            engine.Init(props);
        }

        public string RenderTemplate(TemplateEnum template, IDictionary<string, object> data)
        {
            Template vmTemplate = engine.GetTemplate(templateNameDictionary[(int)template]);
            //template.Encoding = Encoding.UTF8.BodyName;
            var context = new VelocityContext();

            IDictionary<string, object> templateData = data ?? new Dictionary<string, object>();
            foreach (var key in templateData.Keys)
            {
                context.Put(key, templateData[key]);
            }

            using (var writer = new StringWriter())
            {
                vmTemplate.Merge(context, writer);
                return writer.GetStringBuilder().ToString();
            }
        }
    }

    public class ErrorMessage
    {
        public NVelocityTemplateRepository.TemplateEnum Template { get; set; }
        public Exception Exception { get; set; }
        public string Message { get; set; }
    }
}
