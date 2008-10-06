shared float4x4 world;
shared float4x4 view;
shared float4x4 projection;
shared float3 cameraPosition;

//light properties
shared float3 lightPosition;
shared float4 ambientLightColor;
shared float4 diffuseLightColor;
shared float4 specularLightColor;
float4 emmissive;

//material properties
shared float specularPower;
shared float specularIntensity;

shared Texture xTexture0;
sampler TextureSampler = sampler_state { texture = <xTexture0> ; magfilter = LINEAR; minfilter = LINEAR; mipfilter=LINEAR; AddressU = Mirror; AddressV = Mirror;};


struct VertexShaderOutputPerPixelDiffuse
{
     float4 Position : POSITION;
     float3 WorldNormal : TEXCOORD0;
     float3 WorldPosition : TEXCOORD1;
     float2 TextureCoordinate : TEXCOORD2;
};

struct PixelShaderInputPerPixelDiffuse
{
     float3 WorldNormal : TEXCOORD0;
     float3 WorldPosition : TEXCOORD1;
     float2 TextureCoordinate : TEXCOORD2;
};


VertexShaderOutputPerPixelDiffuse PerPixelDiffuseVS(
     float3 position : POSITION,
     float2 texCoord : TEXCOORD0,
     float3 normal : NORMAL )
{
     VertexShaderOutputPerPixelDiffuse output;

     //generate the world-view-projection matrix
     float4x4 wvp = mul(mul(world, view), projection);
     
     //transform the input position to the output
     output.Position = mul(float4(position, 1.0), wvp);

     output.WorldNormal =  mul(normal, world);
     float4 worldPosition =  mul(float4(position, 1.0), world);
     output.WorldPosition = worldPosition / worldPosition.w;
     
     output.TextureCoordinate = texCoord;

     //return the output structure
     return output;
}


float4 DiffuseAndPhongPS(PixelShaderInputPerPixelDiffuse input) : COLOR
{
     //calculate per-pixel diffuse
     float3 directionToLight = normalize(lightPosition - input.WorldPosition);
     float diffuseIntensity = saturate( dot(directionToLight, input.WorldNormal));
     float4 diffuse = diffuseLightColor * diffuseIntensity;

     //calculate Phong components per-pixel
     float3 reflectionVector = normalize(reflect(-directionToLight, input.WorldNormal));
     float3 directionToCamera = normalize(cameraPosition - input.WorldPosition);
     
     //calculate specular component
     float4 specular = specularLightColor * specularIntensity * 
                       pow(saturate(dot(reflectionVector, directionToCamera)), 
                           specularPower);
          
     float4 color = ambientLightColor;
     color.a = 1.0;
     
     color += tex2D(TextureSampler, input.TextureCoordinate);     
     return color* emmissive*1.0f + specular + diffuse;
}

technique Pixel_Diffuse_Pixel_Phong
{
     
    pass P0
    {
          VertexShader = compile vs_2_0 PerPixelDiffuseVS();        
          PixelShader = compile ps_2_0 DiffuseAndPhongPS();
    }
}