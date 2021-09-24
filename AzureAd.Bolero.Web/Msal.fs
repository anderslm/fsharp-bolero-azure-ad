module AzureAd.Bolero.Web.Msal

open Elmish
open Bolero.Remoting.Client
open FSharp.Data

type TokenType = JsonProvider<"""{"uniqueId":"","tenantId":"","tokenType":"access_token","idToken":{"rawIdToken":"","claims":{"aud":"","iss":"https://login.microsoftonline.com","iat":1604669780,"nbf":1604669780,"exp":1604673680,"aio":"","name":"","nonce":"","oid":"","preferred_username":"","rh":"","roles":[],"sub":"","tid":"","uti":"","ver":"2.0"},"issuer":"https://login.microsoftonline.com/","objectId":"","subject":"","tenantId":"","version":"2.0","preferredName":"","name":"","nonce":"","expiration":1604673680},"idTokenClaims":{"aud":"","iss":"https://login.microsoftonline.com/","iat":1604669780,"nbf":1604669780,"exp":1604673680,"aio":"","name":"","nonce":"","oid":"","preferred_username":"","rh":"","roles":[],"sub":"","tid":"","uti":"","ver":"2.0"},"accessToken":"","scopes":[""],"expiresOn":"2020-11-06T14:41:22.000Z","account":{"accountIdentifier":"","homeAccountIdentifier":"","username":"","name":"","idToken":{"aud":"","iss":"https://login.microsoftonline.com/","iat":1604670128,"nbf":1604670128,"exp":1604674028,"aio":"","name":"","nonce":"","oid":"","preferred_username":"","rh":"","roles":[],"sub":"","tid":"","uti":"","ver":"2.0"},"idTokenClaims":{"aud":"","iss":"https://login.microsoftonline.com/","iat":1604670128,"nbf":1604670128,"exp":1604674028,"aio":"","name":"","nonce":"","oid":"","preferred_username":"","rh":"","roles":[],"sub":"","tid":"","uti":"","ver":"2.0"},"environment":"https://login.microsoftonline.com/"},"accountState":""}""">

type Token =
    { accessToken : string
      tokenType   : string }

type Model =
    { Token : TokenType.Root option
      ErrorMessage : string option }

type Msg =
    | AcquireTokenSilent
    | TokenAcquired of TokenType.Root
    | AttemptLogin
    | CouldNotAcquireToken of exn

let init () =
    { Token = None
      ErrorMessage = None }
    , Cmd.ofMsg AcquireTokenSilent // Start off by acquiring a token right away.

let update js msg model =
    match msg with
    | AttemptLogin _ ->
        model, Cmd.OfJS.either js "login" [||] (fun _ -> AcquireTokenSilent) CouldNotAcquireToken

    | AcquireTokenSilent -> 
        model, Cmd.OfJS.either js "acquireTokenSilent" [||] (fun token ->
                                                                let token = TokenType.Parse(token.ToString())
                                                                TokenAcquired token)
                                                            (fun _ -> AttemptLogin)

    | CouldNotAcquireToken exn ->
        { model with ErrorMessage = Some <| "Could not login. " + exn.Message }, Cmd.none

    | TokenAcquired token ->
        { model with Token = Some token }, Cmd.none