﻿[<AutoOpen>]
module internal Dyfrig.Machine.Graph

open Aether
open Dyfrig.Core
open Dyfrig.Core.Operators

(* Graph
        
    Execution runs as a graph of nodes of specific meaning,
    Each node may (depending on type) run some kind of action and
    then provide a way of indicating which node in the graph should
    be invoked next (forming the essential characteristic of processing
    requests as a statemachine).  *)

type Graph =
    Map<string, Node>

and Node =
    | Action of ActionNode
    | Decision of DecisionNode
    | Handler of HandlerNode
    | Operation of OperationNode
    
and ActionNode =
    { Id: string
      Override: Override
      Action: MachineAction
      Next: string }

and DecisionNode =
    { Id: string
      Override: Override
      Decision: MachineDecision
      True: string
      False: string }

and HandlerNode =
    { Id: string
      Override: Override
      Handler: MachineHandler }

and OperationNode =
    { Id: string
      Operation: MachineOperation
      Next: string }

(* Override
       
    Override data is used to be able to provide sensible runtime
    introspection and debugging capabilities,such as integration with future 
    Dyfrig tracing/inspection tools. *)

and Override =
    { Allow: bool
      Overridden: bool }

(* Construction: TODO - Tidy! *)

let construct (definition: MachineDefinition) nodes =
    nodes
    |> List.map (fun n ->
        match n with
        | Action x ->
            x.Id,
            match x.Override.Allow, getPL (actionPLens x.Id) definition with
            | true, Some action -> 
                Action { x with Action = action
                                Override = { x.Override with Overridden = true } }
            | _ -> n
        | Decision x -> 
            x.Id,
            match x.Override.Allow, getPL (decisionPLens x.Id) definition with
            | true, Some decision -> 
                Decision { x with Decision = decision
                                  Override = { x.Override with Overridden = true } }
            | _ -> n
        | Handler x -> 
            x.Id,
            match x.Override.Allow, getPL (handlerPLens x.Id) definition with
            | true, Some handler -> 
                Handler { x with Handler = handler
                                 Override = { x.Override with Overridden = true } }
            | _ -> n
        | Operation x ->
            x.Id, n)
    |> Map.ofList
