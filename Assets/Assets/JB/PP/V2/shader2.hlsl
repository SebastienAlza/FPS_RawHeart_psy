// Paramètres de filtrage


// Taille du texel (doit être calculée dans Shader Graph et passée en paramètre)


// Fonction de filtrage Non-Local Means (NLM)
float3 NonLocalMeans(float2 uv, int kernelSize, float2 texelSize, float sigma)
{
    int halfKernel = (kernelSize - 1) / 2;
    float3 filteredColor = float3(0, 0, 0);
    float weightSum = 0.0;

    // Comparaison des pixels voisins en fonction de la similarité
    for (int y = -halfKernel; y <= halfKernel; y++)
    {
        for (int x = -halfKernel; x <= halfKernel; x++)
        {
            float2 sampleUV = uv + float2(x, y) * texelSize;
            float3 color = SHADERGRAPH_SAMPLE_SCENE_COLOR(sampleUV).rgb;

            // Calcul du poids basé sur la distance
            float2 offset = float2(x, y);
            float dist = length(offset);
            float weight = exp(-dist * dist / (2.0 * sigma * sigma));

            filteredColor += color * weight;
            weightSum += weight;
        }
    }

    return filteredColor / weightSum;
}

// Fonction pour calculer la courbure locale des bords
float Curvature(float2 uv, float2 texelSize)
{
    float3 sobelX = SHADERGRAPH_SAMPLE_SCENE_COLOR(uv + float2(-texelSize.x, 0)).rgb - SHADERGRAPH_SAMPLE_SCENE_COLOR(uv + float2(texelSize.x, 0)).rgb;
    float3 sobelY = SHADERGRAPH_SAMPLE_SCENE_COLOR(uv + float2(0, -texelSize.y)).rgb - SHADERGRAPH_SAMPLE_SCENE_COLOR(uv + float2(0, texelSize.y)).rgb;

    return length(sobelX) + length(sobelY);
}

// Fonction de bruit Perlin modifié
float PerlinNoise(float2 uv)
{
    return frac(sin(dot(uv, float2(12.9898, 78.233))) * 43758.5453);
}

// Fonction de Bloom dynamique
float3 DynamicBloom(float3 color, float bloomThreshold, float bloomStrength)
{
    float brightness = dot(color, float3(0.2126, 0.7152, 0.0722)); // Luminance
    if (brightness < bloomThreshold)
        return color;

    return color + (brightness - bloomThreshold) * bloomStrength;
}

// Fonction de distorsion par intensité de couleur
float2 ColorBasedDistortion(float2 uv, float3 color, float distortionAmplitude)
{
    float intensity = dot(color, float3(0.3, 0.59, 0.11)); // Calcul de la luminance
    return uv + float2(sin(intensity * 10.0) * distortionAmplitude, cos(intensity * 10.0) * distortionAmplitude);
}

// Corps principal du shader
void main_float(float2 uv, float timeFactor, float distortionAmplitude,float bloomThreshold, float bloomStrength, float curvatureStrength, float kernelSize, float sigma, out float3 Result)
{
    //float timeFactor = 0.5;
    //float distortionAmplitude = 0.05;
    //float bloomThreshold = 0.8;
    //float bloomStrength = 0.5;
    //float curvatureStrength = 0.5;
    float2 texelSize = float2(1.0 / 1920, 1.0 / 1080);
    
    // Échantillonnage de la couleur originale
    float3 originalColor = SHADERGRAPH_SAMPLE_SCENE_COLOR(uv).rgb;

    // Application du filtrage Non-Local Means (NLM)
    float3 nlmColor = NonLocalMeans(uv, kernelSize, texelSize, sigma);

    // Calcul de la courbure locale pour ajuster la saturation
    float curvature = Curvature(uv, texelSize);
    float3 curvatureAdjustedColor = nlmColor * (1.0 + curvature * curvatureStrength);

    // Application du bruit Perlin modifié pour introduire des variations aléatoires
    float perlinValue = PerlinNoise(uv);
    float3 perlinColor = curvatureAdjustedColor + float3(perlinValue * 0.1, perlinValue * 0.1, perlinValue * 0.1);

    // Application du Bloom dynamique
    float3 bloomedColor = DynamicBloom(perlinColor, bloomThreshold, bloomStrength);

    // Application de la distorsion par couleur
    float2 distortedUV = ColorBasedDistortion(uv, bloomedColor, distortionAmplitude);
    float3 distortedColor = SHADERGRAPH_SAMPLE_SCENE_COLOR(distortedUV).rgb;

    // Résultat final avec des ajustements et effets multiples
    Result = distortedColor;
}
