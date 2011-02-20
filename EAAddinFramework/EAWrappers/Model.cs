﻿using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Xml;

using UML=TSF.UmlToolingFramework.UML;

namespace TSF.UmlToolingFramework.Wrappers.EA {
  public class Model : UML.UMLModel {
    private global::EA.Repository wrappedModel;

    /// Creates a model connecting to the first running instance of EA
    public Model(){
      object obj = Marshal.GetActiveObject("EA.App");
      global::EA.App eaApp = obj as global::EA.App;
      wrappedModel = eaApp.Repository;
    }

    /// constructor creates EAModel based on the given repository
    public Model(global::EA.Repository eaRepository){
      wrappedModel = eaRepository;
    }

    /// the Element currently selected in EA
    public UML.Classes.Kernel.Element selectedElement {
      get {
        Object selectedItem;
        this.wrappedModel.GetContextItem(out selectedItem);
        return this.factory.createElement(selectedItem);
      }
    }
    
    /// returns the correct type of factory for this model
    public UML.UMLFactory factory {
      get { return Factory.getInstance(this); }
    }

    /// Finds the EA.Element with the given id and returns an EAElementwrapper 
    /// wrapping this element.
    public ElementWrapper getElementWrapperByID(int id){
      try{
        return this.factory.createElement
          (this.wrappedModel.GetElementByID(id)) as ElementWrapper;
      } catch( Exception )  {
        // element not found, return null
        return null;
      }
    }
    
    public UML.Diagrams.Diagram selectedDiagram {
      get {
        return ((Factory)this.factory).createDiagram
          ( this.wrappedModel.GetCurrentDiagram() );
      }
      set { this.wrappedModel.OpenDiagram(((Diagram)value).DiagramID); }
    }
    
    internal Diagram getDiagramByID(int diagramID){
      return ((Factory)this.factory).createDiagram
        ( this.wrappedModel.GetDiagramByID(diagramID) ) as Diagram;
    }

    internal ConnectorWrapper getRelationByID(int relationID) {
      return ((Factory)this.factory).createElement
        ( this.wrappedModel.GetConnectorByID(relationID)) as ConnectorWrapper;
    }
    
    internal List<ConnectorWrapper> getRelationsByQuery(string SQLQuery){
      // get the nodes with the name "Connector_ID"
      XmlDocument xmlrelationIDs = this.SQLQuery(SQLQuery);
      XmlNodeList relationIDNodes = 
        xmlrelationIDs.SelectNodes("//Connector_ID");
      List<ConnectorWrapper> relations = new List<ConnectorWrapper>();
      foreach( XmlNode relationIDNode in relationIDNodes ) {
        int relationID;
        if (int.TryParse( relationIDNode.InnerText, out relationID)) {
          ConnectorWrapper relation = this.getRelationByID(relationID);
          relations.Add(relation);
        }
      }
      return relations;
    }

    /// generic query operation on the model.
    /// Returns results in an xml format
    public XmlDocument SQLQuery(string sqlQuery){
      XmlDocument results = new XmlDocument();
      results.LoadXml(this.wrappedModel.SQLQuery(sqlQuery));
      return results;
    }

    public void saveElement(UML.Classes.Kernel.Element element){
      ((Element)element).save();
    }

    public void saveDiagram(UML.Diagrams.Diagram diagram){
      throw new NotImplementedException();
    }

    internal UML.Classes.Kernel.Element getElementWrapperByPackageID
      (int packageID)
    {
      return this.factory.createElement
        ( this.wrappedModel.GetPackageByID(packageID).Element );
    }

    //returns a list of diagrams according to the given query.
    //the given query should return a list of diagram id's
    internal List<Diagram> getDiagramsByQuery(string sqlGetDiagrams)
    {
        // get the nodes with the name "Diagram_ID"
        XmlDocument xmlDiagramIDs = this.SQLQuery(sqlGetDiagrams);
        XmlNodeList diagramIDNodes =
          xmlDiagramIDs.SelectNodes("//Diagram_ID");
        List<Diagram> diagrams = new List<Diagram>();
        foreach (XmlNode diagramIDNode in diagramIDNodes)
        {
            int diagramID;
            if (int.TryParse(diagramIDNode.InnerText, out diagramID))
            {
                Diagram diagram = this.getDiagramByID(diagramID);
                diagrams.Add(diagram);
            }
        }
        return diagrams;
    }

    internal UML.Classes.Kernel.Operation getOperationByGUID(string guid)
    {
        return this.factory.createElement(this.wrappedModel.GetMethodByGuid(guid)) as UML.Classes.Kernel.Operation;
    }

    public void selectElement(UML.Classes.Kernel.Element element)
    {
        if (element is ElementWrapper){
            this.wrappedModel.ShowInProjectView(((ElementWrapper)element).wrappedElement);
        }
        else if (element is Operation)
        {
            this.wrappedModel.ShowInProjectView(((Operation)element).wrappedOperation);
        }
        else if (element is Attribute)
        {
            this.wrappedModel.ShowInProjectView(((Attribute)element).wrappedAttribute);
        }
        else if (element is Diagram)
        {
            this.wrappedModel.ShowInProjectView(((Diagram)element).wrappedDiagram);
        }
    }
  }
}