﻿//------------------------------------------------------------------------------
// <auto-generated>
//     此代码由工具生成。
//     运行时版本:4.0.30319.296
//
//     对此文件的更改可能会导致不正确的行为，并且如果
//     重新生成代码，这些更改将会丢失。
// </auto-generated>
//------------------------------------------------------------------------------

// 
// 此源代码是由 Microsoft.VSDesigner 4.0.30319.296 版自动生成。
// 
#pragma warning disable 1591

namespace com.Sconit.Service.SAP.MI_POCANCLE_LES {
    using System;
    using System.Web.Services;
    using System.Diagnostics;
    using System.Web.Services.Protocols;
    using System.ComponentModel;
    using System.Xml.Serialization;
    
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Web.Services.WebServiceBindingAttribute(Name="MI_POCANCLE_LESBinding", Namespace="http://www.sih.cq.cn/sap/pp/08")]
    public partial class MI_POCANCLE_LESService : System.Web.Services.Protocols.SoapHttpClientProtocol {
        
        private System.Threading.SendOrPostCallback MI_POCANCLE_LESOperationCompleted;
        
        private bool useDefaultCredentialsSetExplicitly;
        
        /// <remarks/>
        public MI_POCANCLE_LESService() {
            this.Url = global::com.Sconit.Service.SAP.Properties.Settings.Default.com_Sconit_Service_SAP_MI_POCANCLE_LES_MI_POCANCLE_LESService;
            if ((this.IsLocalFileSystemWebService(this.Url) == true)) {
                this.UseDefaultCredentials = true;
                this.useDefaultCredentialsSetExplicitly = false;
            }
            else {
                this.useDefaultCredentialsSetExplicitly = true;
            }
        }
        
        public new string Url {
            get {
                return base.Url;
            }
            set {
                if ((((this.IsLocalFileSystemWebService(base.Url) == true) 
                            && (this.useDefaultCredentialsSetExplicitly == false)) 
                            && (this.IsLocalFileSystemWebService(value) == false))) {
                    base.UseDefaultCredentials = false;
                }
                base.Url = value;
            }
        }
        
        public new bool UseDefaultCredentials {
            get {
                return base.UseDefaultCredentials;
            }
            set {
                base.UseDefaultCredentials = value;
                this.useDefaultCredentialsSetExplicitly = true;
            }
        }
        
        /// <remarks/>
        public event MI_POCANCLE_LESCompletedEventHandler MI_POCANCLE_LESCompleted;
        
        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://sap.com/xi/WebService/soap1.1", RequestElementName="ZSIH_PRODUCTIONCOMFIRECANCLE", RequestNamespace="urn:sap-com:document:sap:rfc:functions", ResponseElementName="ZSIH_PRODUCTIONCOMFIRECANCLE.Response", ResponseNamespace="urn:sap-com:document:sap:rfc:functions", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        [return: System.Xml.Serialization.XmlElementAttribute("OUTPUT", Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string MI_POCANCLE_LES([System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)] ZSPOCOMF_YZ INPUT) {
            object[] results = this.Invoke("MI_POCANCLE_LES", new object[] {
                        INPUT});
            return ((string)(results[0]));
        }
        
        /// <remarks/>
        public void MI_POCANCLE_LESAsync(ZSPOCOMF_YZ INPUT) {
            this.MI_POCANCLE_LESAsync(INPUT, null);
        }
        
        /// <remarks/>
        public void MI_POCANCLE_LESAsync(ZSPOCOMF_YZ INPUT, object userState) {
            if ((this.MI_POCANCLE_LESOperationCompleted == null)) {
                this.MI_POCANCLE_LESOperationCompleted = new System.Threading.SendOrPostCallback(this.OnMI_POCANCLE_LESOperationCompleted);
            }
            this.InvokeAsync("MI_POCANCLE_LES", new object[] {
                        INPUT}, this.MI_POCANCLE_LESOperationCompleted, userState);
        }
        
        private void OnMI_POCANCLE_LESOperationCompleted(object arg) {
            if ((this.MI_POCANCLE_LESCompleted != null)) {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.MI_POCANCLE_LESCompleted(this, new MI_POCANCLE_LESCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }
        
        /// <remarks/>
        public new void CancelAsync(object userState) {
            base.CancelAsync(userState);
        }
        
        private bool IsLocalFileSystemWebService(string url) {
            if (((url == null) 
                        || (url == string.Empty))) {
                return false;
            }
            System.Uri wsUri = new System.Uri(url);
            if (((wsUri.Port >= 1024) 
                        && (string.Compare(wsUri.Host, "localHost", System.StringComparison.OrdinalIgnoreCase) == 0))) {
                return true;
            }
            return false;
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.0.30319.233")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="urn:sap-com:document:sap:rfc:functions")]
    public partial class ZSPOCOMF_YZ {
        
        private string aUFNRField;
        
        private string cONF_TEXTField;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string AUFNR {
            get {
                return this.aUFNRField;
            }
            set {
                this.aUFNRField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string CONF_TEXT {
            get {
                return this.cONF_TEXTField;
            }
            set {
                this.cONF_TEXTField = value;
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
    public delegate void MI_POCANCLE_LESCompletedEventHandler(object sender, MI_POCANCLE_LESCompletedEventArgs e);
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class MI_POCANCLE_LESCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs {
        
        private object[] results;
        
        internal MI_POCANCLE_LESCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) : 
                base(exception, cancelled, userState) {
            this.results = results;
        }
        
        /// <remarks/>
        public string Result {
            get {
                this.RaiseExceptionIfNecessary();
                return ((string)(this.results[0]));
            }
        }
    }
}

#pragma warning restore 1591