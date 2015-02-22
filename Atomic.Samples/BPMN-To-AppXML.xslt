<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:msxsl="urn:schemas-microsoft-com:xslt" exclude-result-prefixes="msxsl"
    xmlns:bpmn="http://www.omg.org/spec/BPMN/20100524/MODEL"
    xmlns:atomic="http://www.atomicplatform.com/Process">
    <xsl:output method="xml" indent="yes"/>                  

  <xsl:template match="/">
      <xsl:for-each select="/bpmn:definitions">
        <xsl:for-each select="bpmn:process">
          <atomic:process id="{@id}" name="{@name}">
            <atomic:events>
              <xsl:if test="bpmn:sequenceFlow/@targetRef = '_start'">
                <atomic:event id="_start">
                  <startOnCondition id="wah" />
                </atomic:event>
              </xsl:if>
              <xsl:if test="bpmn:sequenceFlow/@targetRef = '_stop'">
                <atomic:event id="_stop">
                  <xsl:for-each select="bpmn:sequenceFlow[@targetRef = '_stop']">
                    <startOnCondition id="{@id}" />
                  </xsl:for-each>
                </atomic:event>
              </xsl:if>
            </atomic:events>
            <atomic:tasks>
              <xsl:for-each select="bpmn:scriptTask">
                <atomic:task id="{@id}" name="{@name}">
                  <xsl:variable name="taskID">
                    <xsl:value-of select="@id" />
                  </xsl:variable>
                  <xsl:for-each select="../bpmn:sequenceFlow[@targetRef = $taskID]">
                    <startOnCondition id="{@id}" />
                  </xsl:for-each>
                  <runScript>
                    <xsl:value-of select="bpmn:script" />
                  </runScript>
                </atomic:task>
              </xsl:for-each>
            </atomic:tasks>
            <atomic:conditions>
              <xsl:for-each select="bpmn:sequenceFlow">
                <atomic:taskCondition id="{@id}" name="{@name}">
                  <task id="{@sourceRef}" />
                  <state>Done</state>
                </atomic:taskCondition>
              </xsl:for-each>
            </atomic:conditions>
          </atomic:process>
        </xsl:for-each>
      </xsl:for-each>
    </xsl:template>
</xsl:stylesheet>
