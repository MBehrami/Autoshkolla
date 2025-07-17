export const resetObject=(obj)=>{
    Object.keys(obj).forEach((key)=>{
        obj[key]=''
    })
}