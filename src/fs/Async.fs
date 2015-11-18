module Async

let map f a = async {
    let! result = a
    return f result
}
