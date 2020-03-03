<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="lens.aspx.cs" Inherits="Community.Foundation.ItemLens.ItemView" %>
<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Item Viewer</title>
    <meta name="ROBOTS" content="NOINDEX, NOFOLLOW" />
    <link href="/sitecore/admin/lens-bootstrap.min.css" rel="stylesheet" type="text/css" />
    <style>
        body{
            background-color:#C0C0C0;
        }
        textarea{
            width: 100%;
        }
        .match-group-1 {    
            border: 3px solid green;
            background-color: lightgreen;
        }
        .match-group-2 {    
            border: 3px solid red;
            background-color: lightpink;
        }
        .match-group-3 {    
            border: 3px solid orange;
            background-color: lightgoldenrodyellow;
        }
        .match-group-4 {    
            border: 3px solid blue;
            background-color: lightblue;
        }
        .match-group-5 {  
            /* 5+ gets this style */
            border: 3px solid gray;
            background-color: whitesmoke;
        }
        .duplicate-item {
            color:red;
            font-weight:bold;
        }
        .card-title{
            text-transform:capitalize;
        }
        .spacer {
            height:60px;
        }
		h6.required:after {
		  content:"*";
		  color:red;
		}
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <div class="container-fluid">
            
            <%-- ------------------------------------------------------------------------------------------------------
                                                        Form Input
            ------------------------------------------------------------------------------------------------------ --%>
            <div class="row">
                <div class="col-sm-12">
                    <h2>Selector</h2>
                </div>
            </div>

            <div class="row">
                <div class="col-sm-6">

                    <div class="card">
                      <div class="card-body">
                        <h5 class="card-title">Input</h5> 

                        <%-- Database 1 --%>
                        <h6 class="card-subtitle mb-2 text-muted required">Source Database</h6>
                        <p class="card-text">
                            <asp:DropDownList ID="ddlDatabase1" CssClass="form-control" OnSelectedIndexChanged="sourceInput_Change" AutoPostBack="true" runat="server" />
                        </p>  


                        <%-- Item --%>
                        <h6 class="card-subtitle mb-2 text-muted required">Item</h6>
                        <p class="card-text">
                            <asp:TextBox ID="txtItemId" CssClass="form-control" OnTextChanged="sourceInput_Change" AutoPostBack="true" type="text" runat="server" />
                            <small id="txtItemIdHelp" class="form-text text-muted">Item ID, Item Path, or page URL (if same site context).</small>
                        </p>    
                          
                        <%-- Field --%>
                        <h6 class="card-subtitle mb-2 text-muted">Field</h6>
                        <p class="card-text">
                            <asp:DropDownList ID="ddlField" CssClass="form-control" runat="server" />
                            <small id="txtItemIdHelp" class="form-text text-muted">Compare field value across versions and Sitecore cache versus SQL records.</small>
                        </p>
                                                    
                      </div>
                    </div>

                </div>
                
                <div class="col-sm-6">
                    <div class="card">
                        <div class="card-body">
                            
                        <h5 class="card-title">&nbsp;</h5> 

                        <%-- Database 2 --%>
                        <h6 class="card-subtitle mb-2 text-muted">Comparison Database</h6>
                        <p class="card-text">
                            <asp:DropDownList ID="ddlDatabase2" CssClass="form-control" OnSelectedIndexChanged="sourceInput_Change" AutoPostBack="true" runat="server" />
                        </p>  
                        
                        </div>
                    </div>
                </div>
            </div>
            
            <% if (Community.Foundation.ItemLens.Helpers.Configs.IsUsingSolr) { %>
            <div class="row">
                <div class="col-sm-6">
                    <div class="card">
                        <div class="card-body">
                            <%-- Solr Indexes (left) --%>        
                            <h6 class="card-subtitle mb-2 text-muted">Solr Index (primary)</h6>
                            <p class="card-text">
                                <asp:DropDownList ID="ddlIndex1" CssClass="form-control" runat="server" />
                            </p>  
                                            
                            <h6 class="card-subtitle mb-2 text-muted">Solr Index (additional)</h6>
                            <p class="card-text">
                                <asp:DropDownList ID="ddlIndex3" CssClass="form-control" runat="server" />
                            </p>
                        </div>
                    </div>
                </div>

                <div class="col-sm-6">
                    <div class="card">
                        <div class="card-body">
                            <%-- Solr Indexes (right) --%>        
                            <h6 class="card-subtitle mb-2 text-muted">Solr Index (comparison)</h6>
                            <p class="card-text">
                                <asp:DropDownList ID="ddlIndex2" CssClass="form-control" runat="server" />
                            </p>  
                                            
                            <h6 class="card-subtitle mb-2 text-muted">Solr Index (additional comparison)</h6>
                            <p class="card-text">
                                <asp:DropDownList ID="ddlIndex4" CssClass="form-control" runat="server" />
                            </p>
                        </div>
                    </div>
                </div>
            </div>
            <% } %>
            
            <div class="row">
                <div class="col-sm-12">     
                    <br />
                    <asp:Button text="Submit" CssClass="btn btn-primary" ID="btnSubmit" OnClick="btnSubmit_Click" runat="server" />
                </div>
            </div>

            <%-- ------------------------------------------------------------------------------------------------------
                                                        Viewer
            ------------------------------------------------------------------------------------------------------ --%>
            <div class="row">
                <div class="col-sm-12">
                    <hr />
                    <h2>Viewer</h2>
                    <p>As of <%=DateTime.Now.ToString("MM/dd/yy H:mm:ss") %></p>
                    <asp:Literal ID="litShare" runat="server" />
                </div>
            </div>
            
            <%-- ------------------------------------------------------------------------------------------------------
                                                        API Results
            ------------------------------------------------------------------------------------------------------ --%>
            <div class="row">
                <div class="col-sm-12">
                    <hr />
                    <h2>API Comparison</h2>
                    <p>Compare data from Sitecore API. This can show field values from Sitecore caches.
                </div>
            </div>
            <div class="row">
                <!-- Master -->
                <div class="col-sm-6">
                        <asp:Literal ID="ResultApiMaster" runat="server"></asp:Literal>
                </div>
                
                <!-- Web -->
                <div class="col-sm-6">                    
                        <asp:Literal ID="ResultApiWeb" runat="server"></asp:Literal>
                </div>
            </div>
            
            <%-- ------------------------------------------------------------------------------------------------------
                                                        SQL Results
            ------------------------------------------------------------------------------------------------------ --%>
            <div class="row">
                <div class="col-sm-12">
                    <hr />
                    <h2>SQL Comparison</h2>
                    <p>Compare data from databases. This queries data directly from SQL tables.
                </div>
            </div>
            <div class="row">
                <!-- Master -->
                <div class="col-sm-6">
                        <asp:Literal ID="ResultSqlMaster" runat="server"></asp:Literal>
                </div>
                
                <!-- Web -->
                <div class="col-sm-6">
                        <asp:Literal ID="ResultSqlWeb" runat="server"></asp:Literal>
                </div>
            </div>
                        
            <%-- ------------------------------------------------------------------------------------------------------
                                                        Solr Results
            ------------------------------------------------------------------------------------------------------ --%>
            
            <% if (Community.Foundation.ItemLens.Helpers.Configs.IsUsingSolr) { %>
            <div class="row">
                <div class="col-sm-12">
                    <hr />
                    <h2>Solr Comparison</h2>
                    <p>Compare data from Solr Indexes. This dumps Solr document(s) for current item.
                </div>
            </div>
            <div class="row">

                <!-- Master -->
                <div class="col-sm-6">
                          <asp:Literal runat="server" ID="litIndexesLeft" />
                </div>
                
                <!-- Web -->
                <div class="col-sm-6">
                          <asp:Literal runat="server" ID="litIndexesRight" />
                </div>

            </div>
            <% } %>


        </div><!-- Close fluid-container -->

        <p>

        </p>
    </form>
</body>
</html>
