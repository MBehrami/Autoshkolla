export const camelToPascal=(obj)=> {
    const toPascalCase = (str) => str.charAt(0).toUpperCase() + str.slice(1);
  
    const transformKeys = (obj) => {
      if (Array.isArray(obj)) {
        return obj.map(transformKeys); // Recursively handle arrays
      } else if (obj !== null && typeof obj === 'object') {
        return Object.entries(obj).reduce((acc, [key, value]) => {
          const pascalKey = toPascalCase(key);
          acc[pascalKey] = transformKeys(value); // Recursively handle nested objects
          return acc;
        }, {});
      }
      return obj; // Return non-object types as is
    };
  
    return transformKeys(obj);
}
  