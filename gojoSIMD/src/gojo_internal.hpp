#pragma once

#include "gojoSIMD/gojo.hpp"

#include <algorithm>
#include <cstdint>
#include <limits>
#include <numeric>
#include <queue>
#include <sstream>
#include <string>
#include <tuple>
#include <type_traits>
#include <vector>

namespace gojo::simd {

template <class Unsigned>
constexpr std::size_t laneCount()
{
    if constexpr (sizeof(Unsigned) == 1)
    {
        return 32;
    }
    else if constexpr (sizeof(Unsigned) == 2)
    {
        return 16;
    }
    else if constexpr (sizeof(Unsigned) == 4)
    {
        return 8;
    }
    else
    {
        return 4;
    }
}

template <class Signed>
Signed clampSigned(std::int64_t value)
{
    const auto lo = static_cast<std::int64_t>(std::numeric_limits<Signed>::min());
    const auto hi = static_cast<std::int64_t>(std::numeric_limits<Signed>::max());
    return static_cast<Signed>(std::clamp(value, lo, hi));
}

template <>
inline std::int64_t clampSigned<std::int64_t>(std::int64_t value)
{
    return value;
}

template <class Unsigned>
Unsigned clampUnsigned(std::uint64_t value)
{
    const auto hi = static_cast<std::uint64_t>(std::numeric_limits<Unsigned>::max());
    return static_cast<Unsigned>(std::min(value, hi));
}

inline std::int64_t clampToInt64(std::uint64_t value)
{
    const auto hi = static_cast<std::uint64_t>(std::numeric_limits<std::int64_t>::max());
    return static_cast<std::int64_t>(std::min(value, hi));
}

inline int signOf(std::int64_t value)
{
    return (value > 0) - (value < 0);
}

inline std::int64_t saturatingAdd(std::int64_t left, std::int64_t right)
{
    if (right > 0 && left > std::numeric_limits<std::int64_t>::max() - right)
    {
        return std::numeric_limits<std::int64_t>::max();
    }

    if (right < 0 && left < std::numeric_limits<std::int64_t>::min() - right)
    {
        return std::numeric_limits<std::int64_t>::min();
    }

    return left + right;
}

inline std::int64_t saturatingMultiply(std::int64_t left, std::int64_t right)
{
    if (left == 0 || right == 0)
    {
        return 0;
    }

    const auto max = std::numeric_limits<std::int64_t>::max();
    const auto min = std::numeric_limits<std::int64_t>::min();

    if (left == -1 && right == min)
    {
        return max;
    }

    if (right == -1 && left == min)
    {
        return max;
    }

    const auto product = left * right;
    if (product / right == left)
    {
        return product;
    }

    return ((left < 0) ^ (right < 0)) ? min : max;
}

inline std::uint64_t saturatingMultiply(std::uint64_t left, std::uint64_t right)
{
    if (left != 0 && right > std::numeric_limits<std::uint64_t>::max() / left)
    {
        return std::numeric_limits<std::uint64_t>::max();
    }

    return left * right;
}

inline bool containsRelation(const std::vector<std::uint32_t>& relations, std::uint32_t relation)
{
    return std::find(relations.begin(), relations.end(), relation) != relations.end();
}

inline std::string bitsLabel(std::uint8_t bits)
{
    return std::to_string(static_cast<int>(bits));
}

} // namespace gojo::simd
